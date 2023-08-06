export const measureText = await document.fonts.ready.then(() => {
  const canvas = document.createElement("canvas");
  const context = canvas.getContext("2d");
  return (text, cssFontString) => {
    context.font = cssFontString;
    console.log(context.font, document.fonts.check(context.font));
    return context.measureText(text).width;
  };
});

const units = new Map([
  ["year", (1000 * 60 * 60 * 24 * 1461) / 4],
  ["month", (1000 * 60 * 60 * 24 * 1461) / 4 / 12],
  ["day", 1000 * 60 * 60 * 24],
  ["hour", 1000 * 60 * 60],
  ["minute", 1000 * 60],
]);

const rtf = new Intl.RelativeTimeFormat("en", { numeric: "auto" });

export const getRelativeTime = (d1, d2 = new Date()) => {
  const elapsed = d1 - d2;

  for (const [unit, ms] of units.entries())
    if (Math.abs(elapsed) > ms)
      return rtf.format(Math.round(elapsed / ms), unit);
  return "Just now";
};

export const runAsync = (func) => new Promise((resolve) => resolve(func()));
