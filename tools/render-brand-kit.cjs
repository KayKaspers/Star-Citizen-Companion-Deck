'use strict';
const {chromium}=require('playwright');const options=require('../tests/browser-options.cjs');
const {pathToFileURL}=require('node:url');const path=require('node:path');const fs=require('node:fs');
(async()=>{
 const root=path.resolve(__dirname,'..');const browser=await chromium.launch(options);
 try{
  const page=await browser.newPage();const errors=[];page.on('pageerror',e=>errors.push(e.message));
  for(const [name,width,height] of [['symbol-v2',512,512],['symbol-monochrom-v2',512,512],['symbol-dunkel-v2',512,512],['wortmarke-v2',1200,280],['banner-v2',1600,400],['social-preview-v2',1280,640],['logo-system-v2',2000,2000]]){
   await page.setViewportSize({width,height});await page.goto(pathToFileURL(path.join(root,'branding/assets',name+'.svg')).href);
   await page.evaluate(()=>document.fonts.ready);
   const outside=await page.evaluate(()=>[...document.querySelectorAll('text')].filter(e=>{const r=e.getBoundingClientRect();return r.left<0||r.top<0||r.right>innerWidth+1||r.bottom>innerHeight+1;}).map(e=>e.textContent));
   if(outside.length)throw Error(name+': Text außerhalb des Exports: '+outside.join(', '));
   await page.screenshot({path:path.join(root,'branding/assets',name+'.png'),omitBackground:true});
   console.log(`${name}: ${width} × ${height}, Text innerhalb des Exports`);
  }
  await page.setViewportSize({width:1440,height:1000});await page.goto(pathToFileURL(path.join(root,'branding/VORSCHAU.html')).href);
  await page.evaluate(()=>Promise.all([...document.images].map(i=>i.decode())));
  fs.mkdirSync(path.join(root,'artifacts/branding'),{recursive:true});
  await page.screenshot({path:path.join(root,'artifacts/branding/brand-kit-v2.png'),fullPage:true});
  if(errors.length)throw Error(errors.join('\n'));
  console.log('Brand-Kit V2 gerendert und geprüft.');
 }finally{await browser.close();}
})().catch(e=>{console.error(e);process.exitCode=1;});
