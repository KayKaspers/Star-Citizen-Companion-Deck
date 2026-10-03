'use strict';
// Installed Edge on Windows; Playwright Chromium elsewhere. Both are overridable.
const executablePath = process.env.XEE_BROWSER_PATH || process.env.XEE_CHROME;
module.exports = executablePath
  ? {headless:true, executablePath}
  : {headless:true, ...(process.env.XEE_BROWSER_CHANNEL || process.platform === 'win32'
    ? {channel:process.env.XEE_BROWSER_CHANNEL || 'msedge'} : {})};
