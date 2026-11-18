/* Drawings that are easier to generate than to type. Runs once, before the player starts. */
window.EXPLAINER.draw = function (mk, byId) {
  /* scene 4: a wall of text */
  var wall = byId("wall2"), widths = [240, 220, 250, 200, 240, 250, 180, 230, 250, 210, 240, 160];
  widths.forEach(function (w, n) { mk("path", { "class": "s thin", d: "M155 " + (225 + n * 32) + " h" + w }, wall); });
  /* scene 6: the three parts of the folder */
  var parts = byId("parts"), names = [["the player", "THE PAGE'S MACHINERY"], ["the kit", "STOCK DRAWINGS"], ["the skill", "INSTRUCTIONS FOR CLAUDE"]];
  names.forEach(function (n, k) {
    var y = 145 + k * 155;
    mk("rect", { "class": "el f pop", "data-b": k, "data-hot": k, x: 125, y: y, width: 380, height: 125 }, parts);
    mk("text", { "class": "el", "data-b": k, x: 150, y: y + 58, style: "--d:.2s" }, parts, n[0]);
    mk("text", { "class": "el m", "data-b": k, x: 150, y: y + 98, style: "--d:.2s;font-size:19px" }, parts, n[1]);
  });
};
