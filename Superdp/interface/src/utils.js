export const measureText = await document.fonts.ready.then(() => {
  const canvas = document.createElement("canvas");
  const context = canvas.getContext("2d");
  return (text, cssFontString) => {
    context.font = cssFontString;
    console.log(context.font, document.fonts.check(context.font));
    return context.measureText(text).width;
  };
});

export const runAsync = (func) => new Promise((resolve) => resolve(func()));
