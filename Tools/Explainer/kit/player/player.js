/* Molehunt explainer player.
   Reads window.EXPLAINER = { scenes:[{title, ctx, beats:[text,...]}], sources:[{claim, from}], draw?(mk, byId) }
   and the <g class="scene"> groups inside #stage. An element's attributes say what it does at which beat of its scene:
     data-b="k"    shows from beat k            data-off="k"  hidden again from beat k
     data-dim="k"  dimmed from beat k           data-acc="k"  stamp colour from beat k
     data-hot="k"  stamp colour at beat k only  data-mv="k"   moved by --mx/--my from beat k
   So every beat is a complete state of the picture, and jumping to any beat draws it correctly.
   Capabilities (each optional; the page works without them): tts reads the lines and reports each line's start,
   sample answers questions, db keeps notes and questions.
   Opened as a plain file outside Claude (no window.claude), it reads with the browser's own voice instead,
   one beat at a time, and the question and note boxes say how to switch them on. */
(function(){
"use strict";
var X=window.EXPLAINER,S=X.scenes;
var NS="http://www.w3.org/2000/svg";
function mk(tag,attrs,parent,text){var e=document.createElementNS(NS,tag);for(var k in attrs)e.setAttribute(k,attrs[k]);if(text!=null)e.textContent=text;parent.appendChild(e);return e;}
var $=function(id){return document.getElementById(id);};
if(typeof X.draw==="function")X.draw(mk,$);

var flat=[];S.forEach(function(s,i){s.beats.forEach(function(t,j){flat.push({i:i,j:j,t:t,id:"s"+i+"b"+j});});});
var pos=0,shownScene=-1,playing=false,paused=false,usingVoice=false,runId=0,timer=null;
var tts=null,sample=null,db=null,notes=[],askCtl=null,web=null,webVoices=[],notesClosed=false;
var scenes=[].slice.call(document.querySelectorAll("#stage .scene"));
var bar=$("bar");
S.forEach(function(s,i){var seg=document.createElement("div");seg.className="seg";seg.style.flex=s.beats.length;
  s.beats.forEach(function(_,j){var b=document.createElement("button");b.className="tick";b.type="button";
    var k=flat.findIndex(function(f){return f.i===i&&f.j===j;});
    b.setAttribute("aria-label","Scene "+(i+1)+", "+s.title+", beat "+(j+1));
    b.addEventListener("click",function(){go(k,true);});seg.appendChild(b);});
  bar.appendChild(seg);});
var ticks=[].slice.call(bar.querySelectorAll(".tick"));

function applyEls(g,bi){
  [].forEach.call(g.querySelectorAll("[data-b]"),function(e){var d=e.dataset,on=bi>=+d.b&&!(d.off!=null&&bi>=+d.off);
    e.classList.toggle("on",on);
    if(d.dim!=null)e.classList.toggle("dim",bi>=+d.dim);
    if(d.acc!=null)e.classList.toggle("acc",bi>=+d.acc);
    if(d.mv!=null)e.classList.toggle("mv",bi>=+d.mv);
    if(d.hot!=null)e.classList.toggle("hot",bi===+d.hot);});
}
function render(){
  var f=flat[pos],g=scenes[f.i];
  if(shownScene!==f.i){
    scenes.forEach(function(s,i){s.classList.toggle("cur",i===f.i);if(i!==f.i)[].forEach.call(s.querySelectorAll(".on"),function(e){e.classList.remove("on");});});
    shownScene=f.i;var p=pos;
    requestAnimationFrame(function(){requestAnimationFrame(function(){if(p===pos)applyEls(g,flat[pos].j);});});
  } else applyEls(g,f.j);
  $("caption").textContent=f.t;
  $("caption").classList.toggle("off",!$("cc").checked);
  ticks.forEach(function(t,k){t.classList.toggle("cur",k===pos);t.classList.toggle("past",k<pos);});
  $("where").textContent=(f.i+1)+" / "+S.length+" · "+S[f.i].title;
  $("play").textContent=playing?"Pause":(pos>=flat.length-1?"Replay":"Play");
  renderNotes();
}
function dur(t){return Math.max(2800,t.split(/\s+/).length*345+700);}
function vstat(m){$("vstatus").textContent=m||"";}
function voice(){return (tts||web)&&$("voice").value?$("voice").value:"";}

function start(){
  runId++;var my=runId;clearTimeout(timer);paused=false;render();
  var v=voice();
  if(v&&web){usingVoice=true;web.cancel();setTimeout(function(){if(my===runId&&playing)speakWeb(my);},80);}
  else if(v){
    usingVoice=true;
    var lines=flat.slice(pos).map(function(f){return {text:f.t,id:f.id};});
    tts.speak(lines,{voice:v,language:"en"}).then(function(){
      if(my!==runId)return;playing=false;usingVoice=false;vstat("");render();
    },function(e){
      if(my!==runId)return;usingVoice=false;
      if(e&&e.code==="stopped"){playing=false;vstat("");render();return;}
      voiceFail(e);if(playing)tick(my);
    });
  } else {usingVoice=false;tick(my);}
}
/* the browser's own voice, for a copy opened outside Claude: one beat an utterance, the next when it ends */
function speakWeb(my){
  var f=flat[pos],u=new SpeechSynthesisUtterance(f.t),sel=$("voice").value,done=false,began=Date.now();
  if(sel!=="d"&&webVoices[+sel])u.voice=webVoices[+sel];
  u.lang=(u.voice&&u.voice.lang)||"en-US";
  function next(){if(done)return;done=true;clearTimeout(guard);if(my!==runId||!playing)return;
    if(Date.now()-began<Math.min(900,dur(f.t)/4)){   /* "finished" far too soon: this browser isn't really speaking */
      usingVoice=false;$("voice").value="";$("cc").checked=true;vstat("This browser's voice didn't start. Playing with captions.");render();tick(my);return;}
    if(pos>=flat.length-1){playing=false;usingVoice=false;render();return;}
    pos++;render();speakWeb(my);}
  var guard=setTimeout(next,dur(f.t)*2+5000);      /* some browsers never report the end */
  u.onend=next;u.onerror=next;
  web.speak(u);
}
function tick(my){
  timer=setTimeout(function(){
    if(my!==runId||!playing)return;
    if(pos>=flat.length-1){playing=false;render();return;}
    pos++;render();tick(my);
  },dur(flat[pos].t));
}
function voiceFail(e){
  var c=e&&e.code;
  if(c==="not_granted"||c==="unavailable"){$("voice").value="";$("voice").disabled=true;vstat("The voice isn't available in this window. Playing with captions.");}
  else if(c==="rate_limited")vstat("The voice has hit its limit for now. Playing with captions.");
  else vstat("The voice couldn't start. Playing with captions.");
  $("cc").checked=true;render();
}
function stopVoice(){if(web)web.cancel();else if(tts&&tts.active&&tts.active())tts.stop().catch(function(){});usingVoice=false;paused=false;}
function playPause(){
  if(playing){playing=false;clearTimeout(timer);
    if(usingVoice&&web){runId++;web.cancel();usingVoice=false;}
    else if(usingVoice){paused=true;tts.pause().catch(function(){});}
    else runId++;
    render();return;}
  if(pos>=flat.length-1){pos=0;}
  playing=true;
  if(paused&&usingVoice&&tts&&tts.active()){paused=false;render();tts.resume().catch(function(){start();});}
  else start();
}
function go(k,user){
  k=Math.max(0,Math.min(flat.length-1,k));pos=k;
  if(playing)start();
  else{if(user){runId++;stopVoice();}render();}
}
$("play").addEventListener("click",playPause);
$("prev").addEventListener("click",function(){go(pos-1,true);});
$("next").addEventListener("click",function(){go(pos+1,true);});
$("cc").addEventListener("change",render);
$("voice").addEventListener("change",function(){vstat("");if(playing)start();});
document.addEventListener("keydown",function(e){
  var t=e.target&&e.target.tagName;if(t==="TEXTAREA"||t==="SELECT"||t==="INPUT"||t==="BUTTON"||t==="SUMMARY")return;
  if(e.key===" "){e.preventDefault();playPause();}
  else if(e.key==="ArrowRight"){go(pos+1,true);}
  else if(e.key==="ArrowLeft"){go(pos-1,true);}
});

/* the script and the claims, for reading */
(function(){var box=$("script");S.forEach(function(s,i){var h=document.createElement("h3");h.textContent=(i+1)+". "+s.title;box.appendChild(h);
  var p=document.createElement("p");p.textContent=s.beats.join(" ");box.appendChild(p);});
  var src=$("sources");(X.sources||[]).forEach(function(c){var p=document.createElement("p");p.textContent=c.claim;
    var sp=document.createElement("span");sp.className="src";sp.textContent=c.from;p.appendChild(sp);src.appendChild(p);});
  if(!(X.sources||[]).length)src.parentNode.hidden=true;})();

/* notes, in the page's database */
function renderNotes(){
  var box=$("notes"),sc=flat[pos].i;box.textContent="";
  notes.filter(function(n){return n.scene===sc;}).forEach(function(n){
    var row=document.createElement("div");row.className="note";
    var t=document.createElement("span");t.textContent=n.text;row.appendChild(t);
    var b=document.createElement("button");b.type="button";b.textContent="Remove";
    b.addEventListener("click",function(){b.disabled=true;db.collection("notes").doc(n.id).delete().catch(function(){b.disabled=false;});});
    row.appendChild(b);box.appendChild(row);});
}
$("save").addEventListener("click",function(){
  var text=$("n").value.trim();if(!text||!db)return;
  var f=flat[pos];$("save").disabled=true;$("nstatus").textContent="Saving…";
  db.collection("notes").add({scene:f.i,sceneTitle:S[f.i].title,beat:f.j,line:f.t,text:text,at:new Date().toISOString()})
   .then(function(){$("n").value="";$("nstatus").textContent="Pinned to scene "+(f.i+1)+".";})
   .catch(function(){$("nstatus").textContent="That didn't save. Your text is still here.";})
   .then(function(){$("save").disabled=false;});
});

/* asking Claude */
function askPrompt(q){
  var f=flat[pos],s=S[f.i];
  return "You are answering a question from "+(X.audience||"someone who is not an engineer")+", about a short narrated explainer they are watching. "+
   "Answer in at most four short sentences, in plain words, with no jargon unless you define it in passing. No lists, no headings, no markdown. "+
   "Use only what is below; if it does not cover the question, say so in one sentence and say what they could ask Claude in a normal session instead.\n\n"+
   "THE EXPLAINER: "+X.title+"\n\nTHE WHOLE SCRIPT\n"+S.map(function(x,i){return (i+1)+". "+x.title+": "+x.beats.join(" ")+"\nBackground: "+x.ctx;}).join("\n\n")+
   "\n\nTHEY ARE ON SCENE "+(f.i+1)+" ("+s.title+"), AT THE LINE: \""+f.t+"\"\n\nTHEIR QUESTION: "+q;
}
$("ask").addEventListener("click",function(){
  var q=$("q").value.trim();if(!q||!sample)return;
  if(playing)playPause();
  var f=flat[pos];
  askCtl=new AbortController();$("ask").disabled=true;$("stopask").hidden=false;$("readans").hidden=true;$("answer").textContent="Thinking…";
  sample(askPrompt(q),{signal:askCtl.signal,cache:false,onText:function(u){$("answer").textContent=u.text;}})
   .then(function(r){$("answer").textContent=r.text+(r.truncated?" (cut short)":"");$("readans").hidden=!voice();
     if(db)db.collection("questions").add({scene:f.i,sceneTitle:S[f.i].title,beat:f.j,line:f.t,question:q,answer:r.text.slice(0,1500),at:new Date().toISOString()}).catch(function(){});})
   .catch(function(e){var c=e&&e.code;
     if(c==="cancelled"){$("answer").textContent=(e.text||"");}
     else if(c==="not_granted"||c==="sampling_disabled"||c==="not_declared"||c==="capability_disabled"||c==="capability_removed"){askOff();}
     else if(c==="rate_limited"){$("answer").textContent="Claude is at its limit for now. Try again later.";}
     else{$("answer").textContent=(e&&e.text?e.text+" ":"")+"That didn't come through. Try again.";}})
   .then(function(){$("ask").disabled=false;$("stopask").hidden=true;});
});
$("stopask").addEventListener("click",function(){if(askCtl)askCtl.abort();});
$("readans").addEventListener("click",function(){
  var v=voice(),t=$("answer").textContent.slice(0,1900);if(!v||!t)return;
  runId++;playing=false;paused=false;usingVoice=false;clearTimeout(timer);render();
  if(!tts)return;
  tts.speak(t,{voice:v,language:"en"}).catch(function(){});
});
function askOff(m){$("ask").disabled=true;$("q").disabled=true;$("answer").textContent="";$("askhint").textContent=m||"Questions aren't available in this window.";}
function notesOff(m){notesClosed=true;$("save").disabled=true;$("n").disabled=true;$("nstatus").textContent=m;}

/* capabilities arrive later; the page works without them */
render();
var C=window.claude;
if(!C){
  /* a plain file, outside Claude */
  askOff("The question box switches on when this page is published in Claude as an artifact.");
  notesOff("Notes are kept when this page is published in Claude as an artifact.");
  var sel0=$("voice");sel0.textContent="";
  if("speechSynthesis" in window){
    web=window.speechSynthesis;
    var fill=function(){
      var all=web.getVoices(),en=all.filter(function(v){return /^en/i.test(v.lang);});webVoices=en.length?en:all;
      var keep=sel0.value;sel0.textContent="";
      var o=document.createElement("option");o.value="d";o.textContent="Computer's voice";sel0.appendChild(o);
      webVoices.forEach(function(v,i){var p=document.createElement("option");p.value=String(i);p.textContent=v.name;sel0.appendChild(p);});
      o=document.createElement("option");o.value="";o.textContent="Off";sel0.appendChild(o);
      var best=webVoices.findIndex(function(v){return /natural|neural|online/i.test(v.name);});
      sel0.value=keep&&keep!=="d"?keep:(best>=0?String(best):"d");
    };
    fill();if(web.addEventListener)web.addEventListener("voiceschanged",fill);
    sel0.disabled=false;
    window.addEventListener("pagehide",function(){web.cancel();});
  } else {var o0=document.createElement("option");o0.value="";o0.textContent="not available here";sel0.appendChild(o0);}
}
function use(name){return C&&C.use?C.use(name).catch(function(){return null;}):Promise.resolve(null);}
if(C)use("tts").then(function(t){
  var sel=$("voice");sel.textContent="";
  if(!t){var o=document.createElement("option");o.value="";o.textContent="not available here";sel.appendChild(o);sel.disabled=true;return;}
  tts=t;
  [["buttery","Buttery"],["airy","Airy"],["glassy","Glassy"],["rounded","Rounded"],["","Off"]].forEach(function(v){var o=document.createElement("option");o.value=v[0];o.textContent=v[1];sel.appendChild(o);});
  sel.disabled=false;
  t.onLine(function(e){var k=flat.findIndex(function(f){return f.id===e.id;});if(k>=0&&usingVoice){pos=k;vstat("");render();}});
  t.onState(function(e){
    if(e.state==="starting"&&usingVoice)vstat("Starting the voice…");
    if(e.state==="idle"&&usingVoice&&(e.reason==="viewer_stopped"||e.reason==="interrupted"||e.reason==="hidden"||e.reason==="time_limit")){
      runId++;playing=false;paused=false;usingVoice=false;vstat("");render();}
  });
});
if(C)use("sample").then(function(s){if(s)sample=s;else askOff("Questions aren't available to you on this page. Publish it in your own Claude to ask.");});
/* someone the page was shared with keeps no notes here: the notes are the owner's */
if(C)use("user").then(function(u){if(!u||!u.canEdit)return;
  Promise.resolve(u.canEdit()).then(function(ok){if(ok===false)notesOff("Notes belong to this page's owner. Publish it in your own Claude to keep your own.");}).catch(function(){});});
if(C)use("db").then(function(d){
  if(notesClosed)return;
  if(!d){notesOff("Notes can't be saved in this window.");return;}
  db=d;$("nstatus").textContent="Loading notes…";
  var slow=setTimeout(function(){if(!notesClosed)$("nstatus").textContent="Notes can't be reached yet.";},10000);
  db.collection("notes").onSnapshot(function(snap){
    notes=snap.docs.map(function(x){var v=x.data()||{};return {id:x.id,scene:v.scene,text:String(v.text||"")};});
    if(!snap.metadata.fromCache){clearTimeout(slow);if(!notesClosed&&/Loading|reached/.test($("nstatus").textContent))$("nstatus").textContent="";}
    renderNotes();
  },function(){clearTimeout(slow);if(!notesClosed)$("nstatus").textContent="Notes can't be reached.";});
});
})();
