'use strict';
(() => {
 const storageKey='gold-portal-event-theme';
 const eventKeys=['october20','halloween','christmas','new-year'];
 let current=null;
 function normalize(settings){
  const enabled=Boolean(settings?.enabled),eventKey=eventKeys.includes(settings?.eventKey)?settings.eventKey:'none';
  return{enabled:enabled&&eventKey!=='none',eventKey:enabled?eventKey:'none',messages:settings?.messages&&typeof settings.messages==='object'?settings.messages:{}};
 }
 function apply(settings,persist=true){
  current=normalize(settings);
  if(document.body){
   document.body.dataset.eventTheme=current.eventKey;
   for(const key of eventKeys)document.body.classList.toggle(`event-${key}`,key===current.eventKey);
  }
  if(persist)try{localStorage.setItem(storageKey,JSON.stringify(current));}catch{}
  return current;
 }
 try{const cached=JSON.parse(localStorage.getItem(storageKey)||'null');if(cached)apply(cached,false);}catch{}
 window.GoldPortalEventTheme={get current(){return current;},apply,refresh:async()=>{
  const response=await fetch('/api/portal/event-theme',{cache:'no-cache'});
  if(!response.ok)throw new Error('Không thể tải cài đặt giao diện.');
  const settings=apply(await response.json());
  window.dispatchEvent(new CustomEvent('gold:portal-event-theme',{detail:settings}));
  return settings;
 }};
 window.GoldPortalEventTheme.refresh().catch(()=>{});
})();
