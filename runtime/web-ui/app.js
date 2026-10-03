'use strict';
(() => {
  const bridge = window.chrome?.webview;
  const text = (id, value) => { document.getElementById(id).textContent = value; };
  const health = value => ({UNKNOWN:'Unbekannt',STARTING:'Startet',READY:'Bereit',DEGRADED:'Eingeschränkt',OFFLINE:'Nicht verbunden',ERROR:'Fehler'}[value] || 'Unbekannt');
  const categories = {SHIP:'Schiff',LOCATION:'Ort',COMBAT:'Kampf',PLAYER:'Spieler',SYSTEM:'System',ERROR:'Fehler'};
  const levels = {Notice:'Hinweis',Info:'Info',Warning:'Warnung',Error:'Fehler',Fatal:'Schwerer Fehler',Debug:'Diagnose',UNKNOWN:'Unbekannt'};
  const codes = {LOG_ATTACHED:'Verbunden',LOG_STARTING:'Startet',LOG_MISSING:'Ereignisdatei fehlt',LOG_READ_UNAVAILABLE:'Datei nicht lesbar',LOG_REATTACHED_OR_REPLACED:'Erneut verbunden',LOG_TRUNCATED_OR_REWRITTEN:'Datei neu begonnen',COMPANION_EVENTS_NOT_CONFIGURED:'Ereignisquelle nicht eingerichtet',EXTERNAL_MISSING:'Begleiterfenster fehlt',EXTERNAL_AMBIGUOUS:'Mehrere Begleiterfenster',EXTERNAL_MINIMIZED:'Begleiter minimiert',EXTERNAL_DISABLED:'Begleiter deaktiviert',EXTERNAL_DETECTED_NOT_POSITIONED:'Begleiter erkannt',PLACEMENT_CONFIRMED:'Position bestätigt',PLACEMENT_UNCONFIRMED:'Position noch nicht bestätigt',DISPLAY_SELECTED:'Bildschirm gewählt',DISPLAY_MISSING:'Bildschirm fehlt',DISPLAY_AMBIGUOUS:'Bildschirm nicht eindeutig',DISPLAY_CLONED:'Bildschirm wird dupliziert'};
  const codeText = value => codes[value] || 'Weitere Diagnose verfügbar';
  const themes = new Set(['neutral','rsi','anvil','aegis','drake','origin','crusader','misc','argo','banu','consolidated','esperia','kruger','mirai','vanduul','aopoa']);
  const themeSelect = document.getElementById('theme-select');
  const setTheme = value => { const theme = themes.has(value) ? value : 'neutral'; document.body.dataset.theme = theme; themeSelect.value = theme; };
  try { setTheme(localStorage.getItem('companion-deck-theme')); } catch { setTheme('neutral'); }
  themeSelect.addEventListener('change', () => { setTheme(themeSelect.value); try { localStorage.setItem('companion-deck-theme', themeSelect.value); } catch {} });
  const companionSelect = document.getElementById('companion-select');
  companionSelect.disabled = !bridge;
  companionSelect.addEventListener('change', () => { if (['Orion','Aurora'].includes(companionSelect.value)) bridge?.postMessage('companion:' + companionSelect.value); });
  let feedState = null, orionState = null, companionName = 'Orion', tab = 'LOG';
  function renderFeed() {
    if (!feedState) return;
    const filter = document.getElementById('feed-filter').value.toLocaleLowerCase();
    const extra = document.getElementById('extra-logs').checked;
    const selected = orionState || {events:[], state:'OFFLINE', code:'COMPANION_EVENTS_NOT_CONFIGURED', generation:0};
    const combined = [...selected.events, ...(extra ? feedState.events : [])];
    combined.sort((a, b) => (Date.parse(a.time) || 0) - (Date.parse(b.time) || 0));
    const rows = combined.filter(row => (tab === 'LOG' || (tab === 'EVENTS' ? row.recognized : row.category === tab))
      && (!filter || `${row.source} ${row.message}`.toLocaleLowerCase().includes(filter)));
    const feed = document.getElementById('feed'); const previousScroll = feed.scrollTop;
    const fragment = document.createDocumentFragment();
    for (const row of rows.slice(-200)) {
      const item = document.createElement('div'); item.className = 'feed-row'; item.dataset.level = row.level;
      const time = document.createElement('span'); time.className = 'row-time';
      time.textContent = row.time ? new Date(row.time).toLocaleTimeString('de-DE', {hour12:false}) : 'Unbekannt';
      const badge = document.createElement('span'); badge.className = 'row-badge'; badge.textContent = `${categories[row.category] || 'System'} · ${levels[row.level] || 'Unbekannt'}${row.historical ? ' · Älter' : ''}`;
      const body = document.createElement('div'); const source = document.createElement('small'); source.textContent = /^(ORION|AURORA) · /.test(row.source || '') ? companionName + ' · Reaktionsereignis' : row.source || 'Unstrukturierte Logzeile';
      const message = document.createElement('p'); message.textContent = row.message; body.append(source, message);
      item.append(time, badge, body); fragment.append(item);
    }
    feed.replaceChildren(fragment);
    feed.scrollTop = document.getElementById('auto-scroll').checked ? feed.scrollHeight : previousScroll;
    text('feed-count', `${rows.length} Meldungen · ${extra ? companionName + ' + Spielprotokoll' : 'Nur ' + companionName}`);
    text('feed-health', `${companionName}: ${health(selected.state)} · ${codeText(selected.code)}${extra ? ` · Spielprotokoll: ${health(feedState.state)}` : ''}`);
  }
  for (const button of document.querySelectorAll('[data-tab]')) button.addEventListener('click', () => {
    tab = button.dataset.tab;
    for (const other of document.querySelectorAll('[data-tab]')) other.setAttribute('aria-pressed', String(other === button));
    renderFeed();
  });
  document.getElementById('feed-filter').addEventListener('input', renderFeed);
  document.getElementById('extra-logs').addEventListener('change', renderFeed);
  function render(state) {
    if (state?.schemaVersion !== 1 || !Array.isArray(state.diagnostics)) return;
    text('health', health(state.state)); document.body.dataset.health = state.state;
    companionName = state.companionName === 'Aurora' ? 'Aurora' : 'Orion'; companionSelect.value = companionName;
    text('companion-heading', 'BEGLEITER // ' + companionName.toUpperCase());
    text('companion-description', 'Deutsche Sprachausgabe · ' + (companionName === 'Aurora' ? 'weibliche' : 'männliche') + ' Stimme');
    const d = state.display;
    text('display', d ? `${d.name} · ${d.bounds.width} × ${d.bounds.height} px · (${d.bounds.x}, ${d.bounds.y}) · DPI ${d.dpi || 'UNKNOWN'}` : 'Kein eindeutig ausgewähltes Display');
    text('mode', state.mode === 'Fullscreen' ? 'Vollbild' : 'Fenstermodus');
    const b = state.companionBay;
    text('bay', b ? `${b.width} × ${b.height} px · (${b.x}, ${b.y})` : 'Wartet auf Bildschirm');
    const list = document.getElementById('diagnostics'); list.replaceChildren();
    for (const item of state.diagnostics) { const li = document.createElement('li'); li.title = item.code; li.textContent = `${({display:'Bildschirm',host:'Anwendung',externalWindow:'Begleiter',gameLog:'Spielprotokoll',companionEvents:'Begleiter-Ereignisse',runtime:'Laufzeit',webUi:'Oberfläche'}[item.component] || 'Diagnose')}: ${health(item.state)} · ${codeText(item.code)}`; list.append(li); }
    text('observed', `Beobachtet: ${state.observedAt}`);
    if (state.starCitizen?.schemaVersion === 1 && Array.isArray(state.starCitizen.events)) {
      feedState = state.starCitizen;
      const events = state.companionEvents || state.orionEvents;
      orionState = events?.schemaVersion === 1 && Array.isArray(events.events) ? events : null;
      document.body.classList.add('sc-enabled');
      document.getElementById('game-panel').hidden = false;
      text('heading', 'STAR CITIZEN // LIVE-PROTOKOLL'); renderFeed();
    }
  }
  for (const button of document.querySelectorAll('[data-command]')) {
    button.disabled = !bridge;
    button.addEventListener('click', () => bridge?.postMessage(button.dataset.command));
  }
  if (bridge) { bridge.addEventListener('message', event => render(event.data)); bridge.postMessage('refresh'); }
  else { text('health', 'Unbekannt'); text('display', 'Windows-Anwendung nicht verbunden'); }
})();
