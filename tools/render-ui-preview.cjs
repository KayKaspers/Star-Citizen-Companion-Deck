'use strict';
const {chromium}=require('playwright');const options=require('../tests/browser-options.cjs');const {pathToFileURL}=require('node:url');const path=require('node:path');
(async()=>{const root=path.resolve(__dirname,'..');const browser=await chromium.launch(options);try{const page=await browser.newPage();
    await page.setViewportSize({width:2560,height:720});
    await page.addInitScript(()=>{window.chrome={webview:{addEventListener:(_,f)=>window.receive=f,postMessage:()=>{}}};});
    await page.goto(pathToFileURL(path.join(root,'runtime/web-ui/index.html')).href);
    const state={schemaVersion:1,state:'READY',mode:'Fullscreen',observedAt:'2026-10-03T10:00:00Z',companionName:'Orion',
      display:{name:'XENEON EDGE',bounds:{x:0,y:0,width:2560,height:720},dpi:96},companionBay:{x:1696,y:0,width:864,height:720},diagnostics:[],
      starCitizen:{schemaVersion:1,state:'READY',code:'LOG_ATTACHED',events:[],linesRead:0,droppedLines:0},
      companionEvents:{schemaVersion:1,state:'READY',code:'LOG_ATTACHED',events:[
        {time:'2026-10-03T10:00:00Z',category:'SHIP',level:'Notice',source:'ORION · ship_identified',message:'Schiff erkannt: Anvil C8R Pisces Rescue',historical:false,recognized:true},
        {time:'2026-10-03T10:00:03Z',category:'LOCATION',level:'Notice',source:'ORION · safety_zone_entered',message:'Sicherheitszone betreten',historical:false,recognized:true},
        {time:'2026-10-03T09:59:00Z',category:'PLAYER',level:'Notice',source:'ORION · blueprint_received',message:'Bauplan erhalten: Beispielbauteil',historical:true,recognized:true}]}};
    await page.evaluate(s=>window.receive({data:s}),state);
await page.screenshot({path:path.join(root,'evidence/ui-design-2026-10-03.png')});}finally{await browser.close();}})().catch(e=>{console.error(e);process.exitCode=1;});
