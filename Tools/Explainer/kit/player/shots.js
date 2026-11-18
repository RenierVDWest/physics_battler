// Takes a picture of the stage at every beat of a built explainer, and contact sheets of twelve.
// usage: node shots.js <built.html> <out_dir> [light|dark]
// Look at every sheet before publishing: overlaps, text running out of its box, a beat that changes nothing.
const { chromium } = require('playwright');
const fs = require('fs'), path = require('path');
(async () => {
  const [src, out, scheme] = [process.argv[2], process.argv[3], process.argv[4] || 'light'];
  fs.mkdirSync(out, { recursive: true });
  const wrap = b => '<!doctype html><html><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><style>body{margin:0}[hidden]{display:none!important}</style></head><body>' + b + '</body></html>';
  const browser = await chromium.launch();
  const page = await browser.newPage({ viewport: { width: 1000, height: 900 }, colorScheme: scheme });
  const errors = [];
  page.on('pageerror', e => errors.push(String(e)));
  await page.setContent(wrap(fs.readFileSync(src, 'utf8')));
  await page.waitForTimeout(800);
  const n = await page.locator('.tick').count();
  const stage = page.locator('#stage');
  const files = [];
  for (let k = 0; k < n; k++) {
    await page.locator('.tick').nth(k).click();
    await page.waitForTimeout(4200);               // long enough for the slowest staggered drawing
    const f = path.join(out, 'beat' + String(k).padStart(2, '0') + '.png');
    await stage.screenshot({ path: f }); files.push(f);
  }
  const narrow = await browser.newPage({ viewport: { width: 400, height: 800 } });
  await narrow.setContent(wrap(fs.readFileSync(src, 'utf8')));
  const overflow = await narrow.evaluate(() => document.documentElement.scrollWidth > document.documentElement.clientWidth);
  const sheet = await browser.newPage({ viewport: { width: 1920, height: 1440 } });
  for (let s = 0; s * 12 < files.length; s++) {
    const imgs = files.slice(s * 12, s * 12 + 12).map(f => '<img style="width:640px;height:360px;display:block" src="data:image/png;base64,' + fs.readFileSync(f).toString('base64') + '">').join('');
    await sheet.setContent('<body style="margin:0;display:grid;grid-template-columns:repeat(3,640px)">' + imgs + '</body>');
    await sheet.screenshot({ path: path.join(out, 'sheet' + s + '.png') });
  }
  await browser.close();
  console.log(n + ' beats, ' + Math.ceil(files.length / 12) + ' sheets in ' + out + '; page errors: ' + JSON.stringify(errors) + '; sideways scroll at phone width: ' + overflow);
  if (errors.length || overflow) process.exit(1);
})();
