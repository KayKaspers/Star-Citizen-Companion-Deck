'use strict';
const {chromium} = require('playwright');
const options = require('../../tests/browser-options.cjs');
const assert = require('node:assert/strict');
const {pathToFileURL} = require('node:url');
const path = require('node:path');
(async () => {
  const browser = await chromium.launch(options);
  let passed = 0;
  try {
    const page = await browser.newPage();
    const errors = [];
    page.on('pageerror', e => errors.push(e.message));
    await page.goto(pathToFileURL(path.resolve(__dirname, '../web-ui/index.html')).href);
    assert.equal(await page.locator('#health').textContent(), 'Unbekannt');
    assert.equal(await page.locator('button:disabled').count(), 4); passed++;
    await page.addInitScript(() => {
      window.commands = [];
      window.chrome = {webview: {
        addEventListener: (_, handler) => { window.receive = handler; },
        postMessage: command => window.commands.push(command)
      }};
    });
    await page.reload();
    const state = {schemaVersion:1,state:'DEGRADED',mode:'Fullscreen',observedAt:'2026-10-02T12:00:00Z',
      display:{name:'XENEON EDGE',bounds:{x:-2560,y:-720,width:2560,height:720},dpi:144},
      companionBay:{x:-864,y:-720,width:864,height:720},
      diagnostics:[{component:'externalWindow',state:'DEGRADED',code:'EXTERNAL_MISSING'}]};
    await page.evaluate(s => window.receive({data:s}), state);
    assert.equal(await page.locator('#health').textContent(),'Eingeschränkt');
    assert.match(await page.locator('#diagnostics').textContent(),/Begleiterfenster fehlt/); passed++;
    for (const viewport of [{width:2560,height:720},{width:1707,height:480},{width:900,height:420}]) {
      await page.setViewportSize(viewport);
      assert.equal(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth), true);
      assert.equal(await page.evaluate(() => document.documentElement.scrollHeight <= innerHeight), true);
      passed++;
    }
    await page.locator('[data-command=toggle-mode]').click();
    assert.equal(await page.evaluate(() => window.commands.at(-1)), 'toggle-mode'); passed++;
    await page.evaluate(() => window.receive({data:{schemaVersion:99,diagnostics:[]}}));
    assert.equal(await page.locator('#health').textContent(),'Eingeschränkt'); passed++;
    await page.evaluate(s => window.receive({data:{...s,diagnostics:[{component:'<img src=x onerror=alert(1)>',state:'ERROR',code:'literal'}]}}), state);
    assert.equal(await page.locator('#diagnostics img').count(),0); passed++;
    assert.deepEqual(errors, []); passed++;
    const sc = {...state, starCitizen:{schemaVersion:1,state:'READY',code:'LOG_ATTACHED',generation:1,linesRead:2,droppedLines:0,events:[
      {time:'2026-10-03T09:00:00Z',category:'SHIP',level:'Notice',source:'Fixture',message:'Synthetic ship observation',historical:true,recognized:true},
      {time:null,category:'SYSTEM',level:'Unbekannt',source:'',message:'<img src=x onerror=alert(1)>',historical:false,recognized:false}]}};
    await page.evaluate(s => window.receive({data:s}), sc);
    assert.equal(await page.locator('#game-panel').isVisible(),true);
    assert.equal(await page.locator('.feed-row').count(),0);
    assert.match(await page.locator('#feed-health').textContent(), /Ereignisquelle nicht eingerichtet/); passed++;
    sc.orionEvents = {schemaVersion:1,state:'READY',code:'LOG_ATTACHED',generation:1,events:[
      {time:'2026-10-03T09:01:00Z',category:'LOCATION',level:'Notice',source:'ORION · safety_zone_entered',message:'Sicherheitszone betreten',historical:false,recognized:true}]};
    await page.evaluate(s => window.receive({data:s}), sc);
    assert.equal(await page.locator('.feed-row').count(),1);
    assert.match(await page.locator('#feed').textContent(), /Sicherheitszone/); passed++;
    assert.equal(await page.locator('.feed-row p').evaluate(e => getComputedStyle(e).fontSize), '24px'); passed++;
    await page.locator('#extra-logs').check();
    assert.equal(await page.locator('.feed-row').count(),3);
    await page.locator('#extra-logs').uncheck();
    assert.equal(await page.locator('.feed-row').count(),1); passed++;
    await page.locator('#extra-logs').check();
    assert.equal(await page.locator('.feed-row').count(),3); passed++;
    assert.equal(await page.locator('#feed img').count(),0); passed++;
    await page.locator('[data-tab=SHIP]').click();
    assert.equal(await page.locator('.feed-row').count(),1); passed++;
    await page.locator('#feed-filter').fill('absent');
    assert.equal(await page.locator('.feed-row').count(),0); passed++;
    await page.locator('#feed-filter').fill('');
    for (const viewport of [{width:2560,height:720},{width:1707,height:480},{width:900,height:420}]) {
      await page.setViewportSize(viewport);
      assert.equal(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth && document.documentElement.scrollHeight <= innerHeight),true); passed++;
      if (viewport.width >= 1100) {
        const credit = await page.locator('.creator-credit').boundingBox();
        assert.ok(Math.abs(credit.x + credit.width / 2 - viewport.width / 2) <= 1, 'creator credit centered'); passed++;
      }
    }
    await page.setViewportSize({width:2560,height:720});
    await page.locator('[data-tab=LOG]').click();
    const aurora = {...sc, companionName:'Aurora', companionEvents:{...sc.orionEvents,
      events:sc.orionEvents.events.map(e => ({...e, source:'AURORA · safety_zone_entered'}))}};
    await page.evaluate(s => window.receive({data:s}), aurora);
    assert.match(await page.locator('#companion-heading').textContent(), /AURORA/);
    assert.match(await page.locator('#feed-count').textContent(), /Aurora/); passed++;
    assert.equal(await page.locator('#companion-select').count(), 0);
    assert.equal(await page.evaluate(() => window.commands.some(c => c.startsWith('companion:'))), false); passed++;
    await page.evaluate(s => window.receive({data:s}), {...aurora, starCitizen:null});
    assert.equal(await page.locator('#game-panel').isVisible(), true);
    assert.equal(await page.locator('.feed-row').count(), aurora.companionEvents.events.length); passed++;
    assert.equal(await page.locator('#extra-logs').isDisabled(), true); passed++;
    await page.evaluate(s => window.receive({data:s}), aurora);
    const luminance = hex => { const channels = hex.match(/[0-9a-f]{2}/gi).map(x => parseInt(x,16)/255).map(v => v <= .04045 ? v/12.92 : ((v+.055)/1.055)**2.4); return channels[0]*.2126+channels[1]*.7152+channels[2]*.0722; };
    for (const theme of ['neutral','rsi','anvil','aegis','drake','origin','crusader','misc','argo','banu','consolidated','esperia','kruger','mirai','vanduul','aopoa']) {
      await page.locator('#theme-select').selectOption(theme);
      const colors = await page.evaluate(() => { const css=getComputedStyle(document.body); return Object.fromEntries(['panel','text','muted','accent'].map(k => [k,css.getPropertyValue('--'+k).trim()])); });
      for (const field of ['text','muted','accent']) {
        const a=luminance(colors[field]), b=luminance(colors.panel);
        assert.ok((Math.max(a,b)+.05)/(Math.min(a,b)+.05)>=4.5, `${theme} ${field} contrast`);
      }
      for (const viewport of [{width:2560,height:720},{width:1707,height:480},{width:900,height:420}]) {
        await page.setViewportSize(viewport);
        assert.equal(await page.evaluate(() => document.documentElement.scrollWidth<=innerWidth && document.documentElement.scrollHeight<=innerHeight),true, theme);
      }
      passed++;
    }
    await page.locator('#theme-select').selectOption('drake'); await page.reload();
    assert.equal(await page.locator('body').getAttribute('data-theme'), 'drake'); passed++;
    await page.evaluate(() => localStorage.setItem('companion-deck-theme', '../invalid')); await page.reload();
    assert.equal(await page.locator('body').getAttribute('data-theme'), 'neutral'); passed++;
    assert.deepEqual(errors, []); passed++;
    console.log(`${passed} web UI checks passed`);
  } finally { await browser.close(); }
})().catch(e => { console.error(e); process.exitCode = 1; });
