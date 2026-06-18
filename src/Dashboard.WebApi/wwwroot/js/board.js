/**
 * Enterprise Board — TV Information Display
 * Live clock, auto-refresh, announcement rotation, fullscreen.
 * Zero magic strings — all constants declared at top.
 */
(function () {
  'use strict';

  /* ── Constants ──────────────────────────────────────────── */
  var REFRESH_INTERVAL_SECONDS = 60;
  var CLOCK_UPDATE_MS = 1000;
  var ANNOUNCE_ROTATE_MS = 8000;
  var COUNTDOWN_UPDATE_MS = 1000;
  var CD_LABEL_CLEARS = 'Clears';
  var CD_LABEL_ENDS   = 'Ends';
  var STORAGE_KEY = 'eb-fullscreen';
  var FADE_CLASS = 'eb-fade-in';

  /* ── DOM refs ───────────────────────────────────────────── */
  var clockEl = document.getElementById('eb-clock');
  var dateEl = document.getElementById('eb-date');
  var hijriEl = document.getElementById('eb-hijri');
  var refreshEl = document.getElementById('eb-refresh-time');
  var countdownEls = document.querySelectorAll('[data-countdown]');
  var announceEls = document.querySelectorAll('.announcement-card');
  var fsBtn = document.getElementById('eb-fullscreen-btn');

  /* ── Live clock (Brunei UTC+8) ──────────────────────────── */
  function updateClock() {
    if (!clockEl) return;
    var now = new Date(new Date().toLocaleString('en-US', { timeZone: 'Asia/Brunei' }));
    var h = now.getHours(), m = now.getMinutes(), s = now.getSeconds();
    var ampm = h >= 12 ? 'PM' : 'AM';
    h = h % 12 || 12;
    clockEl.textContent = ('0' + h).slice(-2) + ':' + ('0' + m).slice(-2) + ':' + ('0' + s).slice(-2) + ' ' + ampm;
    if (dateEl) dateEl.textContent = now.toLocaleDateString('en-US', { timeZone: 'Asia/Brunei', weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' });
  }

  /* ── Countdown updater ──────────────────────────────────── */
  function updateCountdowns() {
    var els = document.querySelectorAll('[data-countdown]');
    els.forEach(function (el) {
      var target = parseInt(el.getAttribute('data-countdown'), 10);
      if (!target || target === 0) return;
      var nowUnix = Math.floor(Date.now() / 1000);
      var diff = target - nowUnix;
      var label = el.getAttribute('data-label') || '';
      if (diff <= 0) {
        el.textContent = label === CD_LABEL_CLEARS ? 'Cleared' : (label === CD_LABEL_ENDS ? 'Ended' : 'Now');
        return;
      }
      var d = Math.floor(diff / 86400);
      var h = Math.floor((diff % 86400) / 3600);
      var m = Math.floor((diff % 3600) / 60);
      if (d > 0) {
        el.textContent = label + ' in ' + d + 'd ' + h + 'h';
      } else if (h > 0) {
        el.textContent = label + ' in ' + h + 'h ' + m + 'm';
      } else {
        var s = Math.floor(diff % 60);
        el.textContent = label + ' in ' + m + 'm ' + s + 's';
      }
    });
  }

  /* ── Announcement rotation ──────────────────────────────── */
  var currentAnnounce = 0;
  function rotateAnnouncements() {
    if (!announceEls.length) return;
    announceEls.forEach(function (el) { el.style.display = 'none'; });
    announceEls[currentAnnounce].style.display = '';
    announceEls[currentAnnounce].classList.add(FADE_CLASS);
    currentAnnounce = (currentAnnounce + 1) % announceEls.length;
  }

  /* ── Auto refresh ───────────────────────────────────────── */
  function autoRefresh() {
    var now = new Date();
    if (refreshEl) refreshEl.textContent = new Date().toLocaleTimeString('en-US', { timeZone: 'Asia/Brunei', hour: '2-digit', minute: '2-digit', second: '2-digit', hour12: true });
    // Save scroll position before reload so mobile users aren't thrown to top
    sessionStorage.setItem('eb-scroll-x', window.scrollX);
    sessionStorage.setItem('eb-scroll-y', window.scrollY);
    location.reload();
  }
  // Restore scroll position after auto-refresh
  (function() {
    var sx = sessionStorage.getItem('eb-scroll-x');
    var sy = sessionStorage.getItem('eb-scroll-y');
    if (sx !== null && sy !== null) {
      window.scrollTo(parseInt(sx) || 0, parseInt(sy) || 0);
      sessionStorage.removeItem('eb-scroll-x');
      sessionStorage.removeItem('eb-scroll-y');
    }
  })();

  /* ── Fullscreen toggle ──────────────────────────────────── */
  function toggleFullscreen() {
    var board = document.querySelector('.enterprise-board');
    if (!board) return;
    if (document.fullscreenElement) {
      document.exitFullscreen();
      localStorage.setItem(STORAGE_KEY, '0');
    } else {
      board.requestFullscreen().catch(function () { /* browser may block */ });
      localStorage.setItem(STORAGE_KEY, '1');
    }
  }

  /* ── Keyboard shortcut ──────────────────────────────────── */
  function onKeyDown(e) {
    if (e.key === 'f' || e.key === 'F') {
      if (e.ctrlKey || e.metaKey) return; // don't trap Ctrl+F
      toggleFullscreen();
    }
  }

  /* ── Theme toggle (Enterprise Board only) ──────────────── */
  function initThemeToggle() {
    var btn   = document.getElementById('eb-theme-btn');
    var iconL = document.getElementById('eb-theme-icon-light');
    var iconD = document.getElementById('eb-theme-icon-dark');
    var label = document.getElementById('eb-theme-label');
    if (!btn) return;

    function updateUI(t) {
      if (t === 'light') {
        if (iconL) iconL.style.display = '';
        if (iconD) iconD.style.display = 'none';
        if (label) label.textContent = 'Light';
      } else {
        if (iconL) iconL.style.display = 'none';
        if (iconD) iconD.style.display = '';
        if (label) label.textContent = 'Dark';
      }
    }

    var current = document.documentElement.getAttribute('data-theme');
    updateUI(current === 'light' ? 'light' : 'dark');

    btn.addEventListener('click', function () {
      var next = document.documentElement.getAttribute('data-theme') === 'light' ? 'dark' : 'light';
      document.documentElement.setAttribute('data-theme', next);
      localStorage.setItem('eb-theme', next);
      updateUI(next);
    });
  }

  /* ── Init ───────────────────────────────────────────────── */
  function init() {
    initThemeToggle();
    updateClock();
    setInterval(updateClock, CLOCK_UPDATE_MS);

    updateCountdowns();
    setInterval(updateCountdowns, COUNTDOWN_UPDATE_MS);

    if (announceEls.length > 1) {
      rotateAnnouncements();
      setInterval(rotateAnnouncements, ANNOUNCE_ROTATE_MS);
    }

    if (fsBtn) fsBtn.addEventListener('click', toggleFullscreen);
    document.addEventListener('keydown', onKeyDown);

    // ── Click-to-edit: navigate to Calendar page with event drawer open ──
    document.querySelector('.enterprise-board')?.addEventListener('click', function (e) {
        var card = e.target.closest('[data-event-id]');
        if (!card) return;
        var eventId = card.getAttribute('data-event-id');
        if (eventId) {
            window.location.href = '/Calendar?editEvent=' + encodeURIComponent(eventId);
        }
    });

    // Click-to-view: deadline cards link to projects
    document.querySelector('.enterprise-board')?.addEventListener('click', function (e) {
        var card = e.target.closest('[data-project-deadline]');
        if (!card) return;
        var title = card.getAttribute('data-project-deadline');
        if (title) {
            window.location.href = '/Projects?search=' + encodeURIComponent(title);
        }
    });

    // Auto-refresh timer
    setTimeout(autoRefresh, REFRESH_INTERVAL_SECONDS * 1000);

    // Restore fullscreen if previously active
    if (localStorage.getItem(STORAGE_KEY) === '1') {
      var board = document.querySelector('.enterprise-board');
      if (board) board.requestFullscreen().catch(function () {});
    }
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
  } else {
    init();
  }
})();
