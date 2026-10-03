'use strict';
// Reproduzierbare Vektor-Komposition; das generierte Hintergrundbild bleibt unverändert.
const fs=require('node:fs');const path=require('node:path');
const root=path.resolve(__dirname,'..'), assets=path.join(root,'branding/assets');
const font=name=>fs.readFileSync(path.join(root,'branding/fonts',name)).toString('base64');
const bg=fs.readFileSync(path.join(assets,'orbital-background.png')).toString('base64');
const fonts=`@font-face{font-family:Exo2;src:url(data:font/ttf;base64,${font('Exo2-Variable.ttf')});font-weight:100 900}@font-face{font-family:Inter;src:url(data:font/ttf;base64,${font('Inter-Variable.ttf')});font-weight:100 900}`;
const text=(x,y,size,value,fill='#F2F5F7',weight=400,spacing=0,family='Inter')=>`<text x="${x}" y="${y}" font-family="${family},sans-serif" font-size="${size}" font-weight="${weight}" letter-spacing="${spacing}" fill="${fill}">${value}</text>`;
const mark=(x,y,size,mono)=>`<g transform="translate(${x} ${y}) scale(${size/128})"><path d="M64 8 18 35v57l25 14 7-12-20-12V43l34-20 24 14 7-12Z" fill="${mono||'#8BD3FF'}"/><path d="m64 120 46-27V36L85 22l-7 12 20 12v39l-34 20-24-14-7 12Z" fill="${mono||'#F5CE70'}"/><path d="m64 40 7 17 17 7-17 7-7 17-7-17-17-7 17-7Z" fill="${mono||'#F2F5F7'}"/></g>`;
const svg=(w,h,body,fonted=true)=>`<svg xmlns="http://www.w3.org/2000/svg" width="${w}" height="${h}" viewBox="0 0 ${w} ${h}"><title>Star Citizen Begleiter-Deck · Brand-Kit V2</title>${fonted?`<style>${fonts}</style>`:''}${body}</svg>`;
const image=(w,h)=>`<image href="data:image/png;base64,${bg}" width="${w}" height="${h}" preserveAspectRatio="xMidYMid slice"/>`;
const frame=(x,y,w,h)=>`<path d="M${x} ${y+28}V${y}H${x+w-28}l28 28V${y+h}H${x}Z" fill="#0C131BCC" stroke="#35424F" stroke-width="2"/><path d="M${x} ${y+30}V${y}h30" fill="none" stroke="#8BD3FF" stroke-width="4"/>`;
const write=(name,w,h,body,fonted=true)=>fs.writeFileSync(path.join(assets,name+'.svg'),svg(w,h,body,fonted));
write('symbol-v2',128,128,mark(0,0,128),false);
write('symbol-monochrom-v2',128,128,mark(0,0,128,'#F2F5F7'),false);
write('symbol-dunkel-v2',128,128,mark(0,0,128,'#101E2C'),false);
write('wortmarke-v2',1200,280,mark(20,35,205)+text(265,92,25,'STAR CITIZEN','#8BD3FF',500,5,'Exo2')+text(260,170,67,'BEGLEITER-DECK','#F2F5F7',700,1,'Exo2')+text(265,222,19,'INTEGRATION FÜR ORION + AURORA','#B4C5D4',400,1)+text(265,255,14,'Externe Tools von Aurora Systems · aurora-systems.online','#B4C5D4'));
write('banner-v2',1600,400,image(1600,400)+`<rect width="1050" height="400" fill="#070D1480"/><path d="M64 42h1010m-1010 318h800" stroke="#8BD3FF" stroke-opacity=".3"/><path d="M64 42h55m745 318h60" stroke="#F5CE70" stroke-width="3"/>`+text(64,75,13,'SCBD // DEUTSCH · XENEON EDGE','#B4C5D4',500,3)+mark(60,129,108)+text(200,144,21,'STAR CITIZEN','#8BD3FF',600,5,'Exo2')+text(194,215,62,'BEGLEITER-DECK','#F2F5F7',700,1,'Exo2')+text(200,264,21,'Deine Ereignisse. Dein Begleiter.','#B4C5D4')+text(200,312,16,'INTEGRATION FÜR ORION + AURORA','#F5CE70',600,1)+text(200,342,14,'Externe Tools von Aurora Systems · aurora-systems.online','#B4C5D4'));
write('social-preview-v2',1280,640,image(1280,640)+`<rect width="720" height="640" fill="#070D14BB"/><path d="M64 45h560m-560 542h560" stroke="#35424F" stroke-width="2"/>`+mark(62,83,120)+text(225,125,15,'SCBD // DOPPELSIGNAL','#B4C5D4',500,2)+text(225,164,15,'INTEGRATION FÜR ORION + AURORA','#F5CE70',600,1)+text(64,269,26,'STAR CITIZEN','#8BD3FF',600,5,'Exo2')+text(60,347,65,'BEGLEITER-','#F2F5F7',700,1,'Exo2')+text(60,424,65,'DECK','#F2F5F7',700,1,'Exo2')+text(64,486,22,'Deine Ereignisse. Dein Begleiter.','#B4C5D4')+text(64,538,16,'DEUTSCH · FÜR DAS XENEON EDGE','#F5CE70',600,1)+text(64,573,13,'Orion + Aurora: Aurora Systems · aurora-systems.online','#B4C5D4'));
let board=`<defs><pattern id="grid" width="80" height="80" patternUnits="userSpaceOnUse"><path d="M80 0H0v80" fill="none" stroke="#26313B" stroke-opacity=".38"/></pattern></defs><rect width="2000" height="2000" fill="#090F16"/><rect width="2000" height="2000" fill="url(#grid)"/>`;
board+=text(96,96,22,'SCBD // IDENTITÄTSSYSTEM','#8BD3FF',500,4)+text(1520,96,18,'BRAND-KIT · V2','#B4C5D4',400,2)+frame(96,144,1808,594)+mark(150,285,288)+text(520,304,28,'STAR CITIZEN','#8BD3FF',600,7,'Exo2')+text(512,414,100,'BEGLEITER-DECK','#F2F5F7',700,1,'Exo2')+text(520,490,27,'Deine Ereignisse. Dein Begleiter.','#B4C5D4')+text(520,600,22,'INTEGRATION FÜR ORION + AURORA','#F5CE70',600,2)+text(520,644,20,'EXTERNE TOOLS VON AURORA SYSTEMS','#B4C5D4',400,1)+text(520,684,17,'aurora-systems.online','#B4C5D4');
for(const [i,label] of ['WORTMARKE','BILDZEICHEN','EIN FARBTON'].entries()){
 const x=96+i*614;board+=frame(x,800,580,600)+text(x+34,850,17,`0${i+1} // ${label}`,'#B4C5D4',500,2);
 board+=mark(x+174,945,232,i===2?'#F2F5F7':undefined);
 if(i===0)board+=text(x+83,1248,35,'BEGLEITER-DECK','#F2F5F7',700,1,'Exo2')+text(x+154,1296,17,'STAR CITIZEN','#8BD3FF',600,3,'Exo2');
 else board+=text(x+65,1300,18,i===1?'ORBITALE SIGNALE · ZENTRALER STERN':'REDUZIERTE ANWENDUNG','#B4C5D4',400,0);
}
board+=text(96,1480,22,'FARBEN &amp; TYPOGRAFIE','#8BD3FF',500,3);
for(const [i,[name,color]] of [['NACHTBLAU','#101E2C'],['EISBLAU','#8BD3FF'],['BERNSTEIN','#F5CE70'],['STERNWEISS','#F2F5F7'],['NEBELGRAU','#B4C5D4']].entries()){
 const x=96+i*365;board+=`<rect x="${x}" y="1520" width="338" height="72" fill="${color}"/>`+text(x,1630,17,name,'#F2F5F7',500,1)+text(x,1665,17,color,'#B4C5D4');
}
board+=text(96,1780,42,'EXO 2 · Präzision in der Form','#F2F5F7',600,0,'Exo2')+text(96,1840,25,'Inter · Klarheit für Ereignisse, Details und Dokumentation','#B4C5D4')+text(96,1930,16,'BEGLEITER-DECK: EIGENE INTEGRATION · ORION UND AURORA: AURORA SYSTEMS','#B4C5D4',400,2);
write('logo-system-v2',2000,2000,board);
console.log('Brand-Kit V2: sieben SVG-Kompositionen erstellt.');
