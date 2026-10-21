'use strict';
const $ = s => document.querySelector(s);
const isAdmin = document.body.dataset.admin === 'true';
const categories = {vaccination:'Tiêm chủng',dermatology:'Da liễu & Thẩm mỹ',rehabilitation:'Phục hồi chức năng',wellness:'Dưỡng sinh',office:'Văn phòng & Nội bộ'};
const platforms = {system:'Hệ thống Gold',facebook:'Facebook',zalo:'Zalo',tiktok:'TikTok',youtube:'YouTube',website:'Website'};
const eventThemes={october20:{title:'Chào mừng ngày Phụ nữ Việt Nam 20/10',description:'Gold trân trọng gửi lời chúc tốt đẹp đến một nửa yêu thương.',preview:'Hoa anh đào rơi, ánh hồng và vàng Gold.',icon:'✿',character:'🌸',image:'/portal/events/womens-day-20-10.png'},halloween:{title:'Không khí Halloween tại Gold',description:'Chúc mọi người một mùa Halloween vui vẻ và an toàn.',preview:'Bí ngô phát sáng, bóng ma tinh nghịch và sắc cam tím.',icon:'🎃',character:'🎃',image:'/portal/events/halloween-banner.png'},christmas:{title:'Merry Christmas · Giáng sinh an lành',description:'Gold chúc bạn một mùa Giáng sinh ấm áp và bình an.',preview:'Bông tuyết rơi và ông già Noel cưỡi tuần lộc.',icon:'❄',character:'🎅🏻🛷🦌',image:'/portal/events/christmas-banner.png'},'new-year':{title:'Chào năm mới · 01/01',description:'Gold chúc bạn một năm mới nhiều sức khỏe và thành công.',preview:'Pháo hoa rực sáng, đèn lồng và ánh vàng may mắn.',icon:'🏮',character:'🏮🎆🏮'},none:{title:'Chủ đề sự kiện đã tắt',description:'Cổng hệ sinh thái Gold đang dùng giao diện tiêu chuẩn.',preview:'Không hiển thị hiệu ứng theo sự kiện.',icon:'·',character:'·'}};
const adminViews = new Set(['manage','stats','news','music','settings']);
function adminViewFromUrl(){const requested=window.location.hash.slice(1);return adminViews.has(requested)?requested:'manage';}
let apps = [], view = isAdmin?adminViewFromUrl():'home', token = '', refreshToken = '', deleteId = null, requestVersion = 0;
let adminSessionRestoring=false;
let savedAdminSession=null;
if(isAdmin){try{savedAdminSession=JSON.parse(sessionStorage.getItem('gold-admin-session')||'null');adminSessionRestoring=Boolean(savedAdminSession?.refreshToken&&(!savedAdminSession.refreshTokenExpiresAt||new Date(savedAdminSession.refreshTokenExpiresAt)>new Date()));}catch{savedAdminSession=null;}}
function saveAdminSession(data){token=data.accessToken;refreshToken=data.refreshToken;try{sessionStorage.setItem('gold-admin-session',JSON.stringify({refreshToken,refreshTokenExpiresAt:data.refreshTokenExpiresAt}));}catch{}}
function forgetAdminSession(){token='';refreshToken='';savedAdminSession=null;adminSessionRestoring=false;try{sessionStorage.removeItem('gold-admin-session');}catch{}}
let statistics = null;
let catalogType = 'all';
let editorViewport = null, deleteViewport = null;
let favorites;
try { const saved=JSON.parse(localStorage.getItem('gold-favorites')||'[]'); favorites=new Set(Array.isArray(saved)?saved:[]); } catch { favorites=new Set(); }
const esc = s => String(s??'').replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
const fold = s => s.normalize('NFD').replace(/[\u0300-\u036f]/g,'').replace(/đ/g,'d').toLowerCase();
let toastTimer;
let refreshPromise=null;
let eventTheme={enabled:true,eventKey:'october20'};
let eventMessages={};
const musicDepartments={all:'Tất cả phòng ban',...categories};
let savedMusicState={};try{savedMusicState=JSON.parse(localStorage.getItem('gold-music-state')||'{}')||{};}catch{}
let musicTracks=[],managedMusicTracks=[],musicDepartment=savedMusicState.department||'all',currentMusicId=savedMusicState.id||null,musicRepeat=savedMusicState.repeat||'all',musicPanelOpen=false,musicResumeTime=Number(savedMusicState.time)||0,musicWantsPlayback=Boolean(savedMusicState.playing),musicQueueRenderKey='',musicDurationCache=new Map();
const musicAudio=$('#music-audio');
function saveMusicState(){try{localStorage.setItem('gold-music-state',JSON.stringify({id:currentMusicId,department:musicDepartment,repeat:musicRepeat,time:musicAudio?.currentTime||0,playing:musicWantsPlayback,hidden:Boolean(savedMusicState.hidden)}));}catch{}}
function eventCopy(key){const info=eventThemes[key]||eventThemes.none,custom=eventMessages[key]||{};return{title:String(custom.title||info.title),description:String(custom.description||info.description)};}
function applyEventTheme(settings){eventTheme={enabled:Boolean(settings?.enabled),eventKey:eventThemes[settings?.eventKey]?settings.eventKey:'none'};if(settings?.messages&&typeof settings.messages==='object')eventMessages=settings.messages;const key=eventTheme.enabled?eventTheme.eventKey:'none';window.GoldPortalEventTheme?.apply(settings);document.body.dataset.eventTheme=key;Object.keys(eventThemes).forEach(name=>document.body.classList.toggle('event-'+name,name===key));const info=eventThemes[key],copy=eventCopy(key),hasArt=Boolean(info.image);if($('#event-banner')){$('#event-banner').hidden=key==='none';$('#event-banner-title').textContent=copy.title;$('#event-banner-description').textContent=copy.description;$('#event-character').textContent=info.character;$('#event-character').hidden=hasArt;if(info.image)$('#event-character-art').src=info.image;else $('#event-character-art').removeAttribute('src');$('#event-character-art').hidden=!hasArt;document.querySelectorAll('.october-flower').forEach(item=>{item.textContent=info.icon;item.hidden=hasArt;});}if($('#event-key'))$('#event-key').value=eventTheme.eventKey;if($('#event-enabled')){$('#event-enabled').setAttribute('aria-pressed',String(eventTheme.enabled));$('#event-enabled').classList.toggle('on',eventTheme.enabled);$('#event-enabled-label').textContent=eventTheme.enabled?'Đang bật':'Đang tắt';}const selectedCopy=eventCopy(eventTheme.eventKey);if($('#event-title-text'))$('#event-title-text').value=selectedCopy.title;if($('#event-description-text'))$('#event-description-text').value=selectedCopy.description;renderEventPreview();}
window.addEventListener('gold:portal-event-theme',event=>applyEventTheme(event.detail));
if(window.GoldPortalEventTheme?.current)applyEventTheme(window.GoldPortalEventTheme.current);
function renderEventPreview(){if(!$('#event-key'))return;const key=$('#event-enabled').getAttribute('aria-pressed')==='true'?$('#event-key').value:'none',info=eventThemes[key]||eventThemes.none,copy=eventCopy(key),title=$('#event-title-text')?.value.trim()||copy.title,description=$('#event-description-text')?.value.trim()||copy.description;$('#event-preview-title').textContent=title;$('#event-preview-copy').textContent=description;$('#event-settings-preview').className='event-settings-preview event-preview-'+key;const art=$('#event-preview-art');if(art){if(info.image)art.src=info.image;else art.removeAttribute('src');art.hidden=!info.image;}}
function toast(message){$('#toast').textContent=message;$('#toast').classList.add('show');clearTimeout(toastTimer);toastTimer=setTimeout(()=>$('#toast').classList.remove('show'),3500);}
function captureViewport(){
 const active=document.activeElement,grid=active?.closest('.card-grid');
 return {x:window.scrollX,y:window.scrollY,focus:active?.dataset.id&&active?.dataset.action?{grid:grid?.id,id:active.dataset.id,action:active.dataset.action}:null};
}
function restoreViewport(position){
 if(!position)return;
 if(position.focus){const scope=document.getElementById(position.focus.grid)||document;const target=[...scope.querySelectorAll('[data-action][data-id]')].find(b=>b.dataset.id===position.focus.id&&b.dataset.action===position.focus.action);target?.focus({preventScroll:true});}
 window.scrollTo({left:position.x,top:position.y,behavior:'instant'});
}
function renderPreservingViewport(position=captureViewport()){render();restoreViewport(position);}
async function reloadAppsPreservingViewport(position){apps=await api(isAdmin?'/api/portal/manage':'/api/portal/apps');renderPreservingViewport(position);}
function adminState(){
 document.body.classList.toggle('login-only',isAdmin&&!token);
 $('#login-panel').hidden=!isAdmin||Boolean(token)||adminSessionRestoring;$('#session-restoring').hidden=!adminSessionRestoring;$('#portal-content').hidden=isAdmin&&!token;
 $('#admin-nav').hidden=!isAdmin||!token;$('#add-app').hidden=!(isAdmin&&token&&view==='manage');
 if($('#logout'))$('#logout').hidden=!token;
 $('#favorites-section').hidden=isAdmin;$('#metrics').hidden=view!=='stats';$('#sort').hidden=!isAdmin;
 $('#stats-period').hidden=view!=='stats';$('#stats-history-note').hidden=view!=='stats';
 $('#report-section').hidden=view!=='stats';
 $('#catalog-section').hidden=isAdmin;$('#category-filter').hidden=!isAdmin;
 $('.filter-row').hidden=!isAdmin;
 $('#app-management-section').hidden=!isAdmin;
 $('#news-admin-panel').hidden=!isAdmin||!token||view!=='news';
 $('#event-settings-panel').hidden=!isAdmin||!token||view!=='settings';
 $('#music-admin-panel').hidden=!isAdmin||!token||view!=='music';
 $('#music-dock').hidden=musicTracks.length===0||Boolean(savedMusicState.hidden);
 $('.portal-layout').hidden=isAdmin&&['news','settings','music'].includes(view);
}
function clearAdmin(){forgetAdminSession();apps=[];statistics=null;requestVersion++;adminState();$('#results').replaceChildren();$('#metrics').replaceChildren();}
async function api(url,options={}){
 const headers={...options.headers};if(token)headers.Authorization='Bearer '+token;
 if(options.body&&!(options.body instanceof FormData))headers['Content-Type']='application/json';
 let r;try{r=await fetch(url,{...options,headers});if(r.status===401&&isAdmin&&refreshToken&&!url.includes('/api/auth/refresh')&&!url.includes('/api/accounts/login')){refreshPromise??=(async()=>{const response=await fetch('/api/auth/refresh',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({refreshToken,deviceName:'Gold Central · Trình duyệt'})});const refreshed=await response.json().catch(()=>null);if(!response.ok||!refreshed?.data?.accessToken)throw new Error('Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.');saveAdminSession(refreshed.data);return true;})().finally(()=>refreshPromise=null);try{await refreshPromise;headers.Authorization='Bearer '+token;r=await fetch(url,{...options,headers});}catch{clearAdmin();throw new Error('Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.');}}}catch(error){if(error instanceof TypeError)throw new Error('Không thể kết nối đến máy chủ. Vui lòng kiểm tra kết nối và thử lại.');throw error;}const data=r.status===204?null:await r.json().catch(()=>null);
 if(!r.ok){if(r.status===401&&isAdmin)clearAdmin();const details=data?.errors&&typeof data.errors==='object'?Object.values(data.errors).flatMap(value=>Array.isArray(value)?value:[value]).filter(value=>typeof value==='string'&&value.trim()).join(' '):'';throw new Error([data?.message||(r.status===403?'Bạn cần tài khoản quản trị để thực hiện thao tác này.':r.status===401?'Vui lòng đăng nhập lại.':'Không thể hoàn tất thao tác. Vui lòng thử lại.'),details].filter(Boolean).join(' '));}return data;
}
async function load(){const version=++requestVersion;$('#results').innerHTML='<div class="empty">Đang tải dữ liệu...</div>';if(view==='stats')$('#metrics').replaceChildren();try{const query=`/api/portal/statistics?from=${encodeURIComponent($('#stats-date-from').value)}&to=${encodeURIComponent($('#stats-date-to').value)}`;const [data,report]=await Promise.all([api(isAdmin?'/api/portal/manage':'/api/portal/apps'),view==='stats'?api(query):Promise.resolve(null)]);if(version!==requestVersion)return;apps=data;statistics=report;if(report){$('#stats-date-from').value=report.rangeStart;$('#stats-date-to').value=report.rangeEnd;$('#stats-date-from').max=report.rangeEnd;$('#stats-date-to').min=report.rangeStart;syncDatePreset();}render();}catch(e){if(version===requestVersion)$('#results').innerHTML=`<div class="empty"><b>Chưa thể tải dữ liệu</b><p>${esc(e.message)}</p><button class="secondary" data-action="retry">Thử lại</button></div>`;}}
function metric(value,label){return `<div class="metric"><strong>${value.toLocaleString('vi-VN')}</strong><span>${label}</span></div>`;}
function renderReportCharts(rows){
 const key='rangeClicks';
 const aggregate=(items,labelOf)=>{const map=new Map();for(const a of items){const label=labelOf(a);if(label)map.set(label,(map.get(label)||0)+a[key]);}return [...map].map(([label,value])=>({label,value})).sort((a,b)=>b.value-a.value||a.label.localeCompare(b.label,'vi')).slice(0,7);};
 const groups=[{title:'Theo lĩnh vực',note:'Lượt truy cập theo phòng ban',items:aggregate(rows,a=>categories[a.category])},{title:'Theo hệ thống',note:'Các ứng dụng nội bộ được truy cập nhiều nhất',items:aggregate(rows.filter(a=>a.platform==='system'),a=>a.name)},{title:'Theo mạng xã hội',note:'Lượt truy cập theo kênh truyền thông',items:aggregate(rows.filter(a=>a.platform!=='system'),a=>platforms[a.platform])}];
 $('#report-charts').innerHTML=groups.map((group,index)=>{const max=Math.max(1,...group.items.map(x=>x.value));return `<article class="report-card"><div class="report-card-heading"><span class="report-index">0${index+1}</span><div><h3>${esc(group.title)}</h3><p>${esc(group.note)}</p></div></div>${group.items.length?`<div class="chart-bars" role="list">${group.items.map(item=>`<div class="chart-row" role="listitem"><div class="chart-label"><span title="${esc(item.label)}">${esc(item.label)}</span><b>${item.value.toLocaleString('vi-VN')}</b></div><div class="chart-track" role="progressbar" aria-label="${esc(item.label)}" aria-valuemin="0" aria-valuemax="${max}" aria-valuenow="${item.value}"><span style="width:${Math.max(item.value?3:0,item.value/max*100)}%"></span></div></div>`).join('')}</div>`:'<p class="chart-empty">Chưa có lượt truy cập trong nhóm này.</p>'}</article>`}).join('');
}
function fallbackMedia(){return '<span class="gold-logo"><img src="/portal/gold-logo.png" alt=""></span>';}
const platformIcons = {
 website:'<svg viewBox="0 0 24 24" aria-hidden="true"><circle cx="12" cy="12" r="9" fill="#eef5e8" stroke="#548244" stroke-width="1.6"/><ellipse cx="12" cy="12" rx="4" ry="9" fill="none" stroke="#548244" stroke-width="1.4"/><path d="M3 12h18M5 7.5h14M5 16.5h14" fill="none" stroke="#548244" stroke-width="1.3"/></svg>',
 facebook:'<svg viewBox="0 0 24 24" aria-hidden="true"><path fill="#1877f2" d="M24 12a12 12 0 1 0-13.9 11.85v-8.38H7.05V12h3.05V9.36c0-3.01 1.79-4.67 4.53-4.67 1.31 0 2.68.23 2.68.23v2.95H15.8c-1.49 0-1.95.93-1.95 1.88V12h3.32l-.53 3.47h-2.79v8.38A12 12 0 0 0 24 12Z"/></svg>',
 zalo:'<img class="zalo-brand-icon" src="/portal/icons/zalo.svg" alt="" aria-hidden="true">',
 tiktok:'<svg viewBox="0 0 24 24" aria-hidden="true"><rect width="24" height="24" rx="6" fill="#17221d"/><path d="M14 5v10a4 4 0 1 1-4-4M14 5c0 3 2 5 5 5" fill="none" stroke="#25f4ee" stroke-width="2.7" transform="translate(-.7 .5)"/><path d="M14 5v10a4 4 0 1 1-4-4M14 5c0 3 2 5 5 5" fill="none" stroke="#fe2c55" stroke-width="2.7" transform="translate(.6 -.3)"/><path d="M14 5v10a4 4 0 1 1-4-4M14 5c0 3 2 5 5 5" fill="none" stroke="#fff" stroke-width="2.1"/></svg>',
 youtube:'<svg viewBox="0 0 28 24" aria-hidden="true"><rect y="3" width="28" height="18" rx="6" fill="#f03"/><path d="m11 8 8 4-8 4Z" fill="#fff"/></svg>',
 instagram:'<svg viewBox="0 0 24 24" aria-hidden="true"><rect x="1" y="1" width="22" height="22" rx="6" fill="#d9348a"/><rect x="5" y="5" width="14" height="14" rx="4" fill="none" stroke="#fff" stroke-width="1.7"/><circle cx="12" cy="12" r="3.3" fill="none" stroke="#fff" stroke-width="1.7"/><circle cx="16.5" cy="7.6" r="1" fill="#fff"/></svg>'
};
function platformBadge(a){
 if(a.platform==='system')return `<span class="category-label"><i class="category-dot ${esc(a.category)}"></i>${esc(categories[a.category])}</span>`;
 const label=platforms[a.platform]||(a.platform==='instagram'?'Instagram':a.platform);
 return `<span class="platform-mark" role="img" aria-label="${esc(label)}" title="${esc(label)}">${platformIcons[a.platform]||'<svg viewBox="0 0 24 24" aria-hidden="true"><path d="m9 15 6-6m-8 3-2 2a4 4 0 0 0 6 6l3-3m-2-10 2-2a4 4 0 0 1 6 6l-3 3" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"/></svg>'}</span>`;
}
function card(a){const ready=Boolean(a.url&&a.isVisible);return `<article class="app-card platform-${esc(a.platform)} ${!ready?'unconfigured':''}" data-app-id="${a.id}">
 ${ready?`<button class="card-link" data-action="visit" data-id="${a.id}" aria-label="Truy cập ${esc(a.name)} (mở trong tab mới)" title="${esc(a.name)} · ${esc(platforms[a.platform]||a.platform)}"></button>`:''}
 <div class="card-media ${a.imageUrl?'has-image':''}">${a.imageUrl?`<img class="cover-image" src="${esc(a.imageUrl)}" alt="" loading="lazy">`:fallbackMedia()}<button class="favorite ${favorites.has(a.id)?'on':''}" data-action="favorite" data-id="${a.id}" aria-label="${favorites.has(a.id)?'Bỏ yêu thích':'Thêm yêu thích'} ${esc(a.name)}" aria-pressed="${favorites.has(a.id)}">${favorites.has(a.id)?'♥':'♡'}</button></div>
 <div class="card-content"><h3>${esc(a.name)}</h3><p class="card-description">${esc(a.description)}</p>
 <div class="card-bottom">${platformBadge(a)}<span class="card-cta ${ready?'ready':''}">${ready?'Truy cập <span aria-hidden="true">↗</span>':'Chờ cấu hình'}</span></div>
 ${view==='manage'?`<div class="manage-meta">Thứ tự ${a.order} · ${a.clicks.toLocaleString('vi-VN')} lượt nhấp · ${a.isVisible?'Đang hiện':'Đang ẩn'}</div><div class="manage-actions"><button class="action-edit" data-action="edit" data-id="${a.id}" title="Chỉnh sửa ${esc(a.name)}"><svg viewBox="0 0 24 24" aria-hidden="true"><path d="m15 5 4 4M4 20l4-.8L19.2 8a2.12 2.12 0 0 0-3-3L5 16.2 4 20Z"/></svg><span>Sửa</span></button><button class="action-toggle" data-action="toggle" data-id="${a.id}" title="${a.isVisible?'Ẩn':'Hiện'} ${esc(a.name)}"><svg viewBox="0 0 24 24" aria-hidden="true">${a.isVisible?'<path d="M2 12s3.6-7 10-7 10 7 10 7-3.6 7-10 7S2 12 2 12Z"/><circle cx="12" cy="12" r="3"/>':'<path d="m3 3 18 18M10.6 5.1A10.8 10.8 0 0 1 12 5c6.4 0 10 7 10 7a17 17 0 0 1-3 3.8M6.2 6.2C3.5 8 2 12 2 12s3.6 7 10 7a10 10 0 0 0 4-.8"/><path d="M9.9 9.9a3 3 0 0 0 4.2 4.2"/>'}</svg><span>${a.isVisible?'Ẩn':'Hiện'}</span></button><button class="delete action-delete" data-action="delete" data-id="${a.id}" title="Xóa ${esc(a.name)}"><svg viewBox="0 0 24 24" aria-hidden="true"><path d="M4 7h16M10 11v6m4-6v6M5 7l1 13h12l1-13M9 7V4h6v3"/></svg><span>Xóa</span></button></div>`:''}</div></article>`;}
function resetFilters(){ $('#search').value='';$('#category-filter').value='all';$('#platform-filter').value='all';catalogType='all';$('#catalog-platform-filter').value='all';$('#catalog-department-filter').value='all'; }
function renderCatalog(matching){
 const department=$('#category-filter').value,platform=$('#catalog-platform-filter').value;
 const scoped=matching.filter(a=>department==='all'||a.category===department);
 const displayed=scoped.filter(a=>catalogType==='all'||(catalogType==='system'?a.platform==='system':a.platform!=='system'&&(platform==='all'||a.platform===platform)));
 $('#catalog-count').textContent=scoped.length;
 $('#system-count').textContent=scoped.filter(a=>a.platform==='system').length;
 $('#social-count').textContent=scoped.filter(a=>a.platform!=='system').length;
 $('#department-title').textContent=department==='all'?'Tất cả đơn vị':categories[department];
 $('#catalog-result-count').textContent=`${displayed.length} ứng dụng`;
 $('#catalog-platform-filter').hidden=catalogType!=='social';
 $('#catalog-department-filter').hidden=catalogType!=='social';$('#catalog-department-filter').value=department;
 document.querySelectorAll('[data-department]').forEach(b=>{const active=b.dataset.department===department;b.setAttribute('aria-selected',String(active));b.tabIndex=active?0:-1;b.querySelector('span').textContent=matching.filter(a=>b.dataset.department==='all'||a.category===b.dataset.department).length;});
 $('#department-panel').setAttribute('aria-labelledby','department-'+department);
 document.querySelectorAll('[data-type]').forEach(b=>b.setAttribute('aria-pressed',String(b.dataset.type===catalogType)));
 $('#catalog-results').innerHTML=displayed.length?displayed.map(card).join(''):'<div class="empty"><b>Chưa có ứng dụng phù hợp</b><p>Đổi loại ứng dụng, đơn vị hoặc từ khóa để xem thêm.</p><button class="secondary" data-action="reset">Xem tất cả ứng dụng</button></div>';
}
function render(){
 const visible=apps.filter(a=>a.isVisible);
 $('#favorite-count').textContent=visible.filter(a=>favorites.has(a.id)).length;
 const search=fold($('#search').value.trim()),cat=$('#category-filter').value,platform=$('#platform-filter').value;
 const matching=apps.filter(a=>fold(a.name+' '+a.description+' '+platforms[a.platform]+' '+categories[a.category]).includes(search));
 let filtered=isAdmin?matching.filter(a=>(platform==='all'||a.platform===platform)&&(cat==='all'||a.category===cat)):matching.filter(a=>cat==='all'||a.category===cat);
 if(!isAdmin)$('#favorite-count').textContent=filtered.filter(a=>favorites.has(a.id)).length;
 const sort=$('#sort').value;filtered.sort(sort==='name'?(a,b)=>a.name.localeCompare(b.name,'vi'):sort==='clicks'?(a,b)=>b.clicks-a.clicks:(a,b)=>a.order-b.order||a.name.localeCompare(b.name,'vi'));
 $('#app-count').textContent=filtered.length;$('#result-count').textContent=`${filtered.length} ứng dụng`;
 if(!isAdmin){const saved=filtered.filter(a=>favorites.has(a.id));$('#favorite-results').innerHTML=saved.length?saved.map(card).join(''):`<div class="favorite-empty"><span aria-hidden="true">♡</span><div><b>${favorites.size?'Không có yêu thích phù hợp với bộ lọc':'Lưu những kết nối bạn thường dùng'}</b><p>${favorites.size?'Thử đổi từ khóa hoặc bỏ bớt bộ lọc.':'Nhấn trái tim trên thẻ để thêm ứng dụng vào danh sách yêu thích.'}</p></div></div>`;}
 if(view==='stats'&&statistics){
  const ids=new Set(filtered.map(a=>a.id)),rows=statistics.items.filter(a=>ids.has(a.id));
  const sum=key=>rows.reduce((s,a)=>s+a[key],0),displayDate=value=>new Date(value+'T00:00:00+07:00').toLocaleDateString('vi-VN',{timeZone:'Asia/Ho_Chi_Minh'});
  const rangeLabel=statistics.rangeStart===statistics.rangeEnd?'Ngày '+displayDate(statistics.rangeStart):'Khoảng '+displayDate(statistics.rangeStart)+' – '+displayDate(statistics.rangeEnd);
  $('#metrics').innerHTML=metric(sum('rangeClicks'),'Lượt truy cập trong khoảng')+metric(rows.filter(a=>a.platform==='system').reduce((s,a)=>s+a.rangeClicks,0),'Hệ thống')+metric(rows.filter(a=>a.platform!=='system').reduce((s,a)=>s+a.rangeClicks,0),'Mạng xã hội')+metric(rows.filter(a=>a.rangeClicks>0).length,'Ứng dụng được truy cập');
  $('#stats-range').textContent=rangeLabel;
  $('#report-range-label').textContent=rangeLabel;
  renderReportCharts(rows);
  rows.sort($('#sort').value==='clicks'?(a,b)=>b.rangeClicks-a.rangeClicks:$('#sort').value==='name'?(a,b)=>a.name.localeCompare(b.name,'vi'):()=>0);
  $('#results').innerHTML=`<div class="stats-panel"><table><thead><tr><th>ỨNG DỤNG</th><th>LĨNH VỰC</th><th>TRẠNG THÁI</th><th>LƯỢT TRUY CẬP TRONG KHOẢNG</th></tr></thead><tbody>${rows.map(a=>`<tr><td><strong>${esc(a.name)}</strong></td><td>${esc(categories[a.category])}</td><td><span class="stats-status ${a.isVisible?'visible':'hidden'}">${a.isVisible?'Đang hiển thị':'Đang ẩn'}</span></td><td><strong class="stats-click-count">${a.rangeClicks.toLocaleString('vi-VN')}</strong></td></tr>`).join('')||'<tr><td colspan="4">Không có dữ liệu phù hợp.</td></tr>'}</tbody></table></div>`;
 }else if(isAdmin){$('#results').innerHTML=filtered.length?filtered.map(card).join(''):'<div class="empty"><b>Không tìm thấy ứng dụng</b><p>Thử từ khóa khác hoặc bỏ bớt bộ lọc.</p><button class="secondary" data-action="reset">Xóa bộ lọc</button></div>';}
 if(!isAdmin){$('#results').replaceChildren();renderCatalog(matching);}
 document.querySelectorAll('.cover-image').forEach(img=>{const fallback=()=>{img.parentElement.classList.remove('has-image');img.insertAdjacentHTML('beforebegin',fallbackMedia());img.remove();};img.addEventListener('error',fallback,{once:true});if(img.complete&&!img.naturalWidth)fallback();});
}
async function switchView(next,{syncHistory=true}={}){
 if(!isAdmin||!token)return;
 if(!adminViews.has(next))next='manage';
 if(syncHistory&&adminViewFromUrl()!==next)history.pushState({adminView:next},'',`${location.pathname}${location.search}#${next}`);
 view=next;document.body.dataset.view=view;adminState();
 document.querySelectorAll('#admin-nav [data-view]').forEach(b=>b.classList.toggle('active',b.dataset.view===view));
 if(next==='news'){await loadNewsAdmin();return;}
 if(next==='settings'){await loadEventSettings();return;}
 if(next==='music'){savedMusicState.hidden=false;renderMusicPlayer();saveMusicState();await loadMusicAdmin();return;}
 $('#page-title').textContent=view==='manage'?'Quản lý ứng dụng':'Thống kê truy cập';
 $('#page-subtitle').textContent=view==='manage'?'Quản lý đường dẫn, ảnh bìa và thứ tự hiển thị.':'Theo dõi lượt nhấp theo ngày, tuần, tháng và tổng.';
 $('#directory-title').firstChild.textContent=view==='stats'?'Thống kê ứng dụng ':'Danh sách ứng dụng ';
 $('#directory-subtitle').textContent=view==='stats'?'Số liệu theo kỳ của ngày được chọn, áp dụng các bộ lọc bên dưới.':'Ứng dụng đang hiển thị xuất hiện trong danh mục theo phòng ban.';
 document.querySelectorAll('#admin-nav [data-view]').forEach(b=>b.classList.toggle('active',b.dataset.view===view));await load();
}
function edit(a){editorViewport=captureViewport();const f=$('#app-form');f.reset();f.elements.id.value=a?.id||'';for(const key of ['name','description','category','platform','url','imageUrl','order'])if(a)f.elements[key].value=a[key]??'';if(!a)f.elements.order.value=Math.max(0,...apps.map(x=>x.order))+1;f.elements.isVisible.checked=a?.isVisible??true;$('#editor-error').textContent='';$('#editor-title').textContent=a?'Chỉnh sửa ứng dụng':'Thêm ứng dụng';$('#editor').showModal();}
$('#admin-nav').addEventListener('click',e=>{const b=e.target.closest('[data-view]');if(b)switchView(b.dataset.view);});
window.addEventListener('popstate',()=>{if(isAdmin&&token)switchView(adminViewFromUrl(),{syncHistory:false});});
async function loadEventSettings(){const error=$('#event-settings-error'),save=$('#save-event-settings');error.textContent='';const settings=window.GoldPortalEventTheme?.current;if(settings)applyEventTheme(settings);save.disabled=false;}
const musicTime=value=>{if(!Number.isFinite(value)||value<0)return'00:00';const minutes=Math.floor(value/60),seconds=Math.floor(value%60);return`${String(minutes).padStart(2,'0')}:${String(seconds).padStart(2,'0')}`;};
const musicSize=value=>value<1024*1024?`${Math.max(1,Math.round(value/1024))} KB`:`${(value/1024/1024).toFixed(1)} MB`;
function renderMusicQueue(playing){const key=JSON.stringify([currentMusicId,playing,musicTracks.map(track=>[track.id,track.title,track.artist,musicDurationCache.get(track.id),track.id===currentMusicId?musicAudio.duration:null])]);if(key===musicQueueRenderKey)return;musicQueueRenderKey=key;$('#music-track-list').innerHTML=musicTracks.length?musicTracks.map((item,index)=>{const duration=item.id===currentMusicId&&Number.isFinite(musicAudio.duration)?musicAudio.duration:musicDurationCache.get(item.id);return`<button type="button" class="music-track-row ${item.id===currentMusicId?'active':''}" data-music-play="${item.id}"><span class="music-track-number">${item.id===currentMusicId&&playing?'<i class="music-playing-bars"><b></b><b></b><b></b></i>':String(index+1).padStart(2,'0')}</span><span class="music-track-row-copy"><strong>${esc(item.title)}</strong><small>${esc(item.artist||musicDepartments[item.department]||'Gold')}</small></span><span class="music-track-duration">${Number.isFinite(duration)&&duration>0?musicTime(duration):'--:--'}</span></button>`}).join(''):'<p class="music-list-empty">Playlist này chưa có bài nhạc. Hãy thử phòng ban khác.</p>';}
function cacheMusicDurations(list){for(const track of list)musicDurationCache.set(track.id,Number.isFinite(track.durationSeconds)&&track.durationSeconds>0?track.durationSeconds:null);}
function renderMusicPlayer(){
 const track=musicTracks.find(item=>item.id===currentMusicId),playing=!musicAudio.paused&&!musicAudio.ended;
 $('#music-dock').hidden=musicTracks.length===0||Boolean(savedMusicState.hidden);
 $('#music-dock').classList.toggle('playing',playing);
 $('#music-player-panel').hidden=!musicPanelOpen;
 $('#music-open').setAttribute('aria-expanded',String(musicPanelOpen));
 $('#music-current-title').textContent=track?.title||'Chọn một bài hát';
 $('#music-current-artist').textContent=track?.artist||musicDepartments[track?.department]||'Playlist nội bộ Gold';
 $('#music-current-status').textContent=playing?'ĐANG PHÁT':track?'TẠM DỪNG':'SẴN SÀNG PHÁT';
 $('#music-mini-title').textContent=track?.title||'Gold Music';
 $('#music-mini-subtitle').textContent=playing?'Đang phát · mở phòng nhạc':track?.artist||'Mở phòng nhạc';
 const playIcon=playing?'<svg viewBox="0 0 24 24" aria-hidden="true"><path d="M7 5h4v14H7zM15 5h4v14h-4z"/></svg>':'<svg viewBox="0 0 24 24" aria-hidden="true"><path d="m9 5 11 7-11 7z"/></svg>';
 $('#music-play').innerHTML=playIcon;$('#music-mini-play').innerHTML=playIcon;
 $('#music-play').setAttribute('aria-label',playing?'Tạm dừng':'Phát nhạc');$('#music-mini-play').setAttribute('aria-label',playing?'Tạm dừng':'Phát nhạc');
 $('#music-repeat').title=musicRepeat==='one'?'Lặp lại bài đang phát':musicRepeat==='all'?'Phát lần lượt, lặp danh sách':'Phát lần lượt, dừng ở cuối danh sách';
 $('#music-repeat').setAttribute('aria-label',musicRepeat==='one'?'Đang bật lặp một bài':musicRepeat==='all'?'Đang bật lặp danh sách':'Đang phát lần lượt, không lặp');
 $('#music-repeat').setAttribute('aria-pressed',String(musicRepeat!=='off'));$('#music-repeat').classList.toggle('active',musicRepeat!=='off');$('#music-repeat-one').hidden=musicRepeat!=='one';
 $('#music-queue-count').textContent=`${musicTracks.length} bài`;
 renderMusicQueue(playing);
 $('#music-current-time').textContent=musicTime(musicAudio.currentTime);$('#music-duration').textContent=musicTime(musicAudio.duration);
 $('#music-progress').value=musicAudio.duration?Math.round(musicAudio.currentTime/musicAudio.duration*1000):0;const miniProgress=musicAudio.duration?Math.round(musicAudio.currentTime/musicAudio.duration*1000):0;$('#music-mini-progress').value=miniProgress;$('#music-mini-progress').style.setProperty('--music-progress',`${miniProgress/10}%`);$('#music-mini-elapsed').textContent=musicTime(musicAudio.currentTime);$('#music-mini-duration').textContent=musicTime(musicAudio.duration);
 $('#music-play').disabled=!musicTracks.length;$('#music-mini-play').disabled=!musicTracks.length;
}
async function loadMusicLibrary(department=musicDepartment){
 musicDepartment=department;$('#music-department').value=department;
 try{musicTracks=await api(`/api/music/tracks?department=${encodeURIComponent(department)}`);cacheMusicDurations(musicTracks);musicQueueRenderKey='';const track=musicTracks.find(item=>item.id===currentMusicId);if(currentMusicId&&!track){musicWantsPlayback=false;musicAudio.pause();musicAudio.removeAttribute('src');musicAudio.load();currentMusicId=null;}else if(track&&!musicAudio.src){musicAudio.src=track.streamUrl;musicAudio.addEventListener('loadedmetadata',()=>{musicAudio.currentTime=Math.min(musicResumeTime,musicAudio.duration||musicResumeTime);if(musicWantsPlayback)musicAudio.play().catch(()=>{});musicResumeTime=0;},{once:true});musicAudio.load();}renderMusicPlayer();}
 catch{musicTracks=[];renderMusicPlayer();}
}
async function playMusicTrack(id){
 const track=musicTracks.find(item=>item.id===id);if(!track)return;
 musicWantsPlayback=true;
 if(currentMusicId!==id||!musicAudio.src){currentMusicId=id;musicAudio.src=track.streamUrl;musicAudio.load();}
 renderMusicPlayer();
 try{await musicAudio.play();renderMusicPlayer();if('mediaSession'in navigator&&'MediaMetadata'in window)navigator.mediaSession.metadata=new MediaMetadata({title:track.title,artist:track.artist||'Gold',album:`Gold Central · ${musicDepartments[track.department]||'Âm nhạc'}`});}
 catch{toast('Trình duyệt đang chặn tự phát. Hãy bấm Phát để nghe nhạc.');renderMusicPlayer();}
}
async function toggleMusicPlayback(){if(musicAudio.paused){const track=currentMusicId||musicTracks[0]?.id;if(track)await playMusicTrack(track);}else{musicWantsPlayback=false;musicAudio.pause();saveMusicState();}renderMusicPlayer();}
async function stepMusic(direction){
 if(!musicTracks.length)return;let index=musicTracks.findIndex(track=>track.id===currentMusicId);
 if(index<0){await playMusicTrack(musicTracks[0].id);return;}
 if(direction<0&&index>=0&&musicAudio.currentTime>3){musicAudio.currentTime=0;renderMusicPlayer();return;}
 index+=direction;const loopList=musicRepeat==='all'||musicDepartment!=='all';
 if(index<0)index=loopList?musicTracks.length-1:0;
 if(index>=musicTracks.length){if(loopList)index=0;else{musicWantsPlayback=false;musicAudio.pause();musicAudio.currentTime=0;saveMusicState();renderMusicPlayer();return;}}
 await playMusicTrack(musicTracks[index].id);
}
function onMusicEnded(){if(musicRepeat==='one'){musicAudio.currentTime=0;musicAudio.play().catch(()=>{});return;}if(musicRepeat==='all'||musicDepartment!=='all'){stepMusic(1);return;}musicWantsPlayback=false;saveMusicState();renderMusicPlayer();}
function formatManagedMusic(){
 const query=fold($('#music-admin-search').value.trim()),department=$('#music-admin-filter').value;
 const rows=managedMusicTracks.filter(track=>(department==='all'||track.department===department)&&fold(`${track.title} ${track.artist||''}`).includes(query));
 $('#music-admin-results').innerHTML=rows.length?`<div class="music-admin-table-wrap"><table class="music-admin-table"><thead><tr><th>BÀI HÁT</th><th>PLAYLIST</th><th>FILE</th><th>TRẠNG THÁI</th><th>THAO TÁC</th></tr></thead><tbody>${rows.map(track=>`<tr><td><strong>${esc(track.title)}</strong><small>${esc(track.artist||'Chưa thêm nghệ sĩ')}</small></td><td><span class="music-department-badge">${esc(musicDepartments[track.department]||track.department)}</span></td><td>${musicSize(track.size)}</td><td><span class="music-status ${track.isPublished?'published':''}">${track.isPublished?'Đang phát':'Đang ẩn'}</span></td><td><div class="music-admin-actions"><button type="button" data-music-action="edit" data-id="${track.id}">Sửa</button><button type="button" data-music-action="toggle" data-id="${track.id}">${track.isPublished?'Ẩn':'Hiện'}</button><button type="button" class="delete" data-music-action="delete" data-id="${track.id}">Xóa</button></div></td></tr>`).join('')}</tbody></table></div>`:'<div class="music-admin-empty"><span>♫</span><b>Chưa có bài hát trong thư viện</b><p>Tải lên MP3 hoặc WAV rồi chọn playlist cho một phòng ban hoặc toàn công ty.</p><button type="button" class="primary" id="music-empty-add">＋ Thêm bài hát</button></div>';
}
async function loadMusicAdmin(){
 $('#music-admin-results').innerHTML='<div class="empty">Đang tải thư viện nhạc...</div>';
 try{managedMusicTracks=await api('/api/music/tracks/manage');formatManagedMusic();}catch(error){$('#music-admin-results').innerHTML=`<div class="empty"><b>Chưa thể tải thư viện nhạc</b><p>${esc(error.message)}</p></div>`;}
}
function editMusic(track){
 const form=$('#music-form');form.reset();form.elements.id.value=track?.id||'';form.elements.title.value=track?.title||'';form.elements.artist.value=track?.artist||'';form.elements.department.value=track?.department||'all';form.elements.sortOrder.value=track?.sortOrder??(Math.max(0,...managedMusicTracks.map(item=>item.sortOrder))+1);form.elements.isPublished.checked=track?.isPublished??true;$('#music-file').required=!track;$('#music-file-name').textContent=track?'Để trống nếu giữ file hiện tại':'MP3 hoặc WAV · tối đa 40 MB';$('#music-editor-title').textContent=track?'Chỉnh sửa bài hát':'Thêm bài hát';$('#music-editor-error').textContent='';$('#music-editor').showModal();
}
function musicFormData(track,published=track.isPublished){const data=new FormData();data.append('title',track.title);data.append('artist',track.artist||'');data.append('department',track.department);data.append('sortOrder',String(track.sortOrder));data.append('isPublished',String(published));return data;}
$('#music-department').addEventListener('change',e=>loadMusicLibrary(e.target.value));
$('#music-open').addEventListener('click',()=>{musicPanelOpen=!musicPanelOpen;renderMusicPlayer();});
$('#music-mini-previous').addEventListener('click',()=>stepMusic(-1));$('#music-mini-next').addEventListener('click',()=>stepMusic(1));
$('#music-mini-close').addEventListener('click',()=>{savedMusicState.hidden=true;musicWantsPlayback=false;musicAudio.pause();musicAudio.currentTime=0;musicAudio.removeAttribute('src');musicAudio.load();renderMusicPlayer();saveMusicState();});
$('#music-close').addEventListener('click',()=>{musicPanelOpen=false;renderMusicPlayer();});
$('#music-play').addEventListener('click',toggleMusicPlayback);$('#music-mini-play').addEventListener('click',toggleMusicPlayback);
$('#music-previous').addEventListener('click',()=>stepMusic(-1));$('#music-next').addEventListener('click',()=>stepMusic(1));
$('#music-repeat').addEventListener('click',()=>{musicRepeat=musicRepeat==='all'?'one':musicRepeat==='one'?'off':'all';renderMusicPlayer();saveMusicState();});
$('#music-track-list').addEventListener('click',e=>{const button=e.target.closest('[data-music-play]');if(button)playMusicTrack(button.dataset.musicPlay);});
const seekMusic=e=>{if(musicAudio.duration)musicAudio.currentTime=musicAudio.duration*Number(e.target.value)/1000;};$('#music-progress').addEventListener('change',seekMusic);$('#music-mini-progress').addEventListener('change',seekMusic);
musicAudio.addEventListener('timeupdate',renderMusicPlayer);musicAudio.addEventListener('durationchange',renderMusicPlayer);musicAudio.addEventListener('play',()=>{musicWantsPlayback=true;renderMusicPlayer();saveMusicState();});musicAudio.addEventListener('pause',()=>{renderMusicPlayer();saveMusicState();});musicAudio.addEventListener('ended',onMusicEnded);function resumeMusicIfNeeded(){if(musicWantsPlayback&&musicAudio.paused&&currentMusicId)playMusicTrack(currentMusicId);else saveMusicState();}document.addEventListener('visibilitychange',()=>{if(document.visibilityState==='hidden')saveMusicState();else resumeMusicIfNeeded();});window.addEventListener('pagehide',saveMusicState);window.addEventListener('pageshow',resumeMusicIfNeeded);setInterval(saveMusicState,5000);
musicAudio.addEventListener('error',()=>{if(currentMusicId)toast('Không phát được file nhạc này. Hãy kiểm tra lại file đã tải lên.');renderMusicPlayer();});
if('mediaSession'in navigator){try{navigator.mediaSession.setActionHandler('play',()=>toggleMusicPlayback());navigator.mediaSession.setActionHandler('pause',()=>{musicWantsPlayback=false;musicAudio.pause();saveMusicState();});navigator.mediaSession.setActionHandler('previoustrack',()=>stepMusic(-1));navigator.mediaSession.setActionHandler('nexttrack',()=>stepMusic(1));}catch{}}
$('#add-music').addEventListener('click',()=>editMusic());
$('#music-file').addEventListener('change',()=>{const file=$('#music-file').files[0];if(file){$('#music-file-name').textContent=`${file.name} · ${musicSize(file.size)}`;if(file.size>40*1024*1024)$('#music-editor-error').textContent='File nhạc không được vượt quá 40 MB.';else $('#music-editor-error').textContent='';}});
$('#music-form').addEventListener('submit',async event=>{
 event.preventDefault();const form=event.currentTarget,button=form.querySelector('[type="submit"]'),id=form.elements.id.value,file=$('#music-file').files[0];$('#music-editor-error').textContent='';
 if(file&&(file.size>40*1024*1024||!['mp3','wav'].includes(file.name.split('.').pop().toLowerCase()))){$('#music-editor-error').textContent='Vui lòng chọn file MP3 hoặc WAV, tối đa 40 MB.';return;}
 if(!id&&!file){$('#music-editor-error').textContent='Vui lòng chọn file nhạc.';return;}
 button.disabled=true;const body=new FormData(form);body.delete('id');if(!file)body.delete('file');
 try{await api(`/api/music/tracks${id?'/'+id:''}`,{method:id?'PUT':'POST',body});$('#music-editor').close();toast(id?'Đã cập nhật bài hát.':'Đã thêm bài hát vào thư viện.');await Promise.all([loadMusicAdmin(),loadMusicLibrary(musicDepartment)]);}
 catch(error){$('#music-editor-error').textContent=error.message;}
 finally{button.disabled=false;}
});
$('#music-admin-search').addEventListener('input',formatManagedMusic);$('#music-admin-filter').addEventListener('change',formatManagedMusic);
$('#music-admin-results').addEventListener('click',async event=>{
 if(event.target.closest('#music-empty-add')){editMusic();return;}
 const button=event.target.closest('[data-music-action]');if(!button)return;const track=managedMusicTracks.find(item=>item.id===button.dataset.id);if(!track)return;
 if(button.dataset.musicAction==='edit'){editMusic(track);return;}
 if(button.dataset.musicAction==='delete'&&!window.confirm(`Xóa “${track.title}” khỏi thư viện?`))return;
 button.disabled=true;
 try{if(button.dataset.musicAction==='delete'){await api(`/api/music/tracks/${track.id}`,{method:'DELETE'});toast('Đã xóa bài hát.');}else{await api(`/api/music/tracks/${track.id}`,{method:'PUT',body:musicFormData(track,!track.isPublished)});toast(track.isPublished?'Đã ẩn bài hát khỏi playlist.':'Đã xuất bản bài hát.');}await Promise.all([loadMusicAdmin(),loadMusicLibrary(musicDepartment)]);}
 catch(error){button.disabled=false;toast(error.message);}
});
$('#event-enabled').addEventListener('click',()=>{const enabled=$('#event-enabled').getAttribute('aria-pressed')!=='true'&&$('#event-key').value!=='none';applyEventTheme({enabled,eventKey:$('#event-key').value});});
$('#event-key').addEventListener('change',()=>{const enabled=$('#event-key').value!=='none';applyEventTheme({enabled,eventKey:$('#event-key').value});});
function updateEventMessage(){const key=$('#event-key').value;if(key!=='none')eventMessages[key]={title:$('#event-title-text').value,description:$('#event-description-text').value};renderEventPreview();}
$('#event-title-text').addEventListener('input',updateEventMessage);
$('#event-description-text').addEventListener('input',updateEventMessage);
$('#event-settings-form').addEventListener('submit',async e=>{e.preventDefault();const save=$('#save-event-settings'),error=$('#event-settings-error');save.disabled=true;error.textContent='';try{const result=await api('/api/portal/event-theme',{method:'PUT',body:JSON.stringify({enabled:$('#event-enabled').getAttribute('aria-pressed')==='true',eventKey:$('#event-key').value,messages:eventMessages})});applyEventTheme(result);toast('Đã lưu chủ đề sự kiện.');}catch(ex){error.textContent=ex.message;}finally{save.disabled=false;}});
for(const selector of ['#category-filter','#sort','#platform-filter'])$(selector).addEventListener('change',render);
$('#search').addEventListener('input',render);
function formatDateValue(value){if(!value)return 'Chọn ngày';const [year,month,day]=value.split('-');return `${day}/${month}/${year}`;}
function syncDateTriggers(){document.querySelectorAll('.date-trigger').forEach(button=>{const input=$('#'+button.dataset.dateTarget);button.querySelector('.date-trigger-value').textContent=formatDateValue(input.value);});}
function syncDatePreset(){const from=$('#stats-date-from').value,to=$('#stats-date-to').value,today=todayVn(),selected=from===to?(from===today?'today':from===shiftDate(today,-1)?'yesterday':''):'';document.querySelectorAll('[data-date-preset]').forEach(button=>{const active=button.dataset.datePreset===selected;button.classList.toggle('active',active);button.setAttribute('aria-pressed',String(active));});syncDateTriggers();}
function refreshDateBounds(changed){const from=$('#stats-date-from'),to=$('#stats-date-to');if(changed==='from'&&from.value>to.value)to.value=from.value;if(changed==='to'&&to.value<from.value)from.value=to.value;from.max=to.value;to.min=from.value;syncDatePreset();load();}
$('#stats-date-from').addEventListener('change',()=>{if($('#stats-date-from').checkValidity())refreshDateBounds('from');});$('#stats-date-to').addEventListener('change',()=>{if($('#stats-date-to').checkValidity())refreshDateBounds('to');});
const todayVn=()=>new Date(Date.now()+7*60*60*1000).toISOString().slice(0,10),shiftDate=(date,days)=>{const value=new Date(date+'T12:00:00+07:00');value.setUTCDate(value.getUTCDate()+days);return value.toISOString().slice(0,10);};
let openDatePicker=null;
const monthNames=['Tháng 1','Tháng 2','Tháng 3','Tháng 4','Tháng 5','Tháng 6','Tháng 7','Tháng 8','Tháng 9','Tháng 10','Tháng 11','Tháng 12'];
function renderDatePicker(field,month){const input=field.querySelector('input[type=date]'),year=month.getUTCFullYear(),monthIndex=month.getUTCMonth(),firstDay=new Date(Date.UTC(year,monthIndex,1)),offset=(firstDay.getUTCDay()+6)%7,start=new Date(Date.UTC(year,monthIndex,1-offset)),min=input.min||'0000-01-01',max=input.max||'9999-12-31';let cells='';for(let i=0;i<42;i++){const date=new Date(start);date.setUTCDate(start.getUTCDate()+i);const value=date.toISOString().slice(0,10),outside=date.getUTCMonth()!==monthIndex,today=value===todayVn(),active=value===input.value;cells+=`<button type="button" class="calendar-day${outside?' outside':''}${today?' today':''}${active?' selected':''}" data-calendar-date="${value}" ${value<min||value>max?'disabled':''} aria-label="${date.getUTCDate()} tháng ${date.getUTCMonth()+1} năm ${date.getUTCFullYear()}" ${active?'aria-pressed="true"':''}>${date.getUTCDate()}</button>`;}field.querySelector('.date-calendar').innerHTML=`<div class="calendar-heading"><button type="button" class="calendar-month-nav" data-calendar-shift="-1" aria-label="Tháng trước"><svg viewBox="0 0 24 24" aria-hidden="true"><path d="m15 18-6-6 6-6"/></svg></button><strong>${monthNames[monthIndex]} ${year}</strong><button type="button" class="calendar-month-nav" data-calendar-shift="1" aria-label="Tháng sau"><svg viewBox="0 0 24 24" aria-hidden="true"><path d="m9 18 6-6-6-6"/></svg></button></div><div class="calendar-weekdays">${['T2','T3','T4','T5','T6','T7','CN'].map(day=>`<span>${day}</span>`).join('')}</div><div class="calendar-grid">${cells}</div><div class="calendar-footer"><button type="button" data-calendar-clear>Xóa ngày</button><button type="button" data-calendar-today>Hôm nay</button></div>`;field.dataset.calendarYear=year;field.dataset.calendarMonth=monthIndex;}
function closeDatePicker(){if(!openDatePicker)return;openDatePicker.classList.remove('calendar-open');openDatePicker.querySelector('.date-trigger').setAttribute('aria-expanded','false');openDatePicker=null;}
function showDatePicker(button){const field=button.closest('.date-field');if(openDatePicker===field){closeDatePicker();return;}closeDatePicker();const value=field.querySelector('input[type=date]').value||todayVn(),[year,month]=value.split('-').map(Number);openDatePicker=field;renderDatePicker(field,new Date(Date.UTC(year,month-1,1)));field.classList.add('calendar-open');button.setAttribute('aria-expanded','true');}
document.querySelectorAll('.date-trigger').forEach(button=>button.addEventListener('click',()=>showDatePicker(button)));
$('.stats-date-inputs').addEventListener('click',event=>{if(!openDatePicker)return;const shift=event.target.closest('[data-calendar-shift]');if(shift){renderDatePicker(openDatePicker,new Date(Date.UTC(Number(openDatePicker.dataset.calendarYear),Number(openDatePicker.dataset.calendarMonth)+Number(shift.dataset.calendarShift),1)));return;}const day=event.target.closest('[data-calendar-date]');if(day){const input=openDatePicker.querySelector('input[type=date]');input.value=day.dataset.calendarDate;closeDatePicker();input.dispatchEvent(new Event('change',{bubbles:true}));return;}if(event.target.closest('[data-calendar-today],[data-calendar-clear]')){const input=openDatePicker.querySelector('input[type=date]'),today=event.target.closest('[data-calendar-today]');input.value=today?todayVn():'';const changed=input.id.endsWith('from')?'from':'to';closeDatePicker();if(input.value)refreshDateBounds(changed);else{syncDateTriggers();load();}}});
document.addEventListener('click',event=>{if(openDatePicker&&!event.target.closest('.date-field'))closeDatePicker();});
$('.stats-date-presets').addEventListener('click',e=>{const preset=e.target.closest('[data-date-preset]')?.dataset.datePreset;if(!preset)return;const date=shiftDate(todayVn(),preset==='yesterday'?-1:0);$('#stats-date-from').value=date;$('#stats-date-to').value=date;refreshDateBounds('preset');});
function selectDepartment(button){$('#category-filter').value=button.dataset.department;$('#catalog-department-filter').value=button.dataset.department;render();}
const departmentLayout=window.matchMedia('(min-width:1100px)');
function updateDepartmentOrientation(){$('#department-tabs').setAttribute('aria-orientation',departmentLayout.matches?'vertical':'horizontal');}
departmentLayout.addEventListener('change',updateDepartmentOrientation);updateDepartmentOrientation();
$('#department-tabs').addEventListener('click',e=>{const button=e.target.closest('[data-department]');if(button)selectDepartment(button);});
$('#department-tabs').addEventListener('keydown',e=>{
 const previous=departmentLayout.matches?'ArrowUp':'ArrowLeft',nextKey=departmentLayout.matches?'ArrowDown':'ArrowRight';
 if(![previous,nextKey,'Home','End'].includes(e.key))return;
 const tabs=[...document.querySelectorAll('[data-department]')],index=tabs.indexOf(e.target);if(index<0)return;
 e.preventDefault();const next=e.key==='Home'?0:e.key==='End'?tabs.length-1:(index+(e.key===nextKey?1:-1)+tabs.length)%tabs.length;
 selectDepartment(tabs[next]);tabs[next].focus();
});
$('#type-filters').addEventListener('click',e=>{const button=e.target.closest('[data-type]');if(button){catalogType=button.dataset.type;render();}});
$('#catalog-platform-filter').addEventListener('change',render);
$('#catalog-department-filter').addEventListener('change',event=>{$('#category-filter').value=event.target.value;render();});
async function handleCardAction(e){
 const b=e.target.closest('[data-action]');if(!b)return;const a=apps.find(x=>x.id===b.dataset.id),position=captureViewport();
 try{switch(b.dataset.action){
 case 'retry':await load();break;
 case 'reset':resetFilters();render();break;
 case 'favorite':favorites.has(a.id)?favorites.delete(a.id):favorites.add(a.id);try{localStorage.setItem('gold-favorites',JSON.stringify([...favorites]));}catch{}renderPreservingViewport(position);break;
 case 'visit':{b.disabled=true;const win=window.open('about:blank','_blank');if(win)win.opener=null;try{const result=await api(`/api/portal/apps/${a.id}/visit`,{method:'POST'});if(win)win.location.href=result.url;else window.location.assign(result.url);a.clicks++;renderPreservingViewport(position);}catch(error){win?.close();b.disabled=false;throw error;}break;}
 case 'edit':if(token)edit(a);break;
 case 'toggle':if(!token)return;b.disabled=true;await api(`/api/portal/apps/${a.id}`,{method:'PUT',body:JSON.stringify({...a,isVisible:!a.isVisible})});await reloadAppsPreservingViewport(position);toast('Đã cập nhật hiển thị.');break;
 case 'delete':if(!token)return;deleteId=a.id;deleteViewport=position;$('#delete-name').textContent=a.name;$('#delete-dialog').showModal();break;
 }}catch(error){b.disabled=false;toast(error.message);}
}
$('#results').addEventListener('click',handleCardAction);$('#favorite-results').addEventListener('click',handleCardAction);
$('#catalog-results').addEventListener('click',handleCardAction);
$('#confirm-delete').onclick=async()=>{const b=$('#confirm-delete');b.disabled=true;try{await api(`/api/portal/apps/${deleteId}`,{method:'DELETE'});$('#delete-dialog').close();await reloadAppsPreservingViewport(deleteViewport);toast('Đã xóa ứng dụng.');}catch(e){toast(e.message);}finally{b.disabled=false;}};
$('#add-app').onclick=()=>{if(isAdmin&&token)edit();};
if($('#logout'))$('#logout').onclick=async()=>{const oldToken=token,oldRefresh=refreshToken;if(oldToken&&oldRefresh)try{await fetch('/api/auth/logout',{method:'POST',headers:{Authorization:'Bearer '+oldToken,'Content-Type':'application/json'},body:JSON.stringify({refreshToken:oldRefresh})});}catch{}clearAdmin();$('#login-form').reset();toast('Đã thoát chế độ quản trị trên trình duyệt này.');};
$('#login-form').onsubmit=async e=>{e.preventDefault();const f=e.target,b=f.querySelector('[type=submit]');b.disabled=true;b.setAttribute('aria-busy','true');b.textContent='Đang đăng nhập...';$('#login-error').textContent='';try{const response=await api('/api/accounts/login',{method:'POST',body:JSON.stringify({username:f.elements.username.value,password:f.elements.password.value})});const data=response.data;if(data.account.role!=='admin')throw new Error('Tài khoản này không có quyền quản trị.');saveAdminSession(data);f.reset();await switchView('manage');toast('Đăng nhập quản trị thành công.');}catch(error){$('#login-error').textContent=error.message;$('#login-error').focus({preventScroll:true});}finally{b.disabled=false;b.removeAttribute('aria-busy');b.innerHTML='Đăng nhập <span>→</span>';}};
$('#app-form').onsubmit=async e=>{e.preventDefault();const f=e.target,b=f.querySelector('[type=submit]');b.disabled=true;$('#editor-error').textContent='';try{
 let imageUrl=f.elements.imageUrl.value;const file=$('#image-file').files[0];if(file){if(file.size>10*1024*1024||!['image/png','image/jpeg'].includes(file.type))throw new Error('Vui lòng chọn ảnh PNG hoặc JPG, tối đa 10 MB.');const form=new FormData();form.append('file',file);const uploaded=await api('/api/files/upload',{method:'POST',body:form});imageUrl=uploaded.data.url;}
 const id=f.elements.id.value,body={name:f.elements.name.value,description:f.elements.description.value,category:f.elements.category.value,platform:f.elements.platform.value,url:f.elements.url.value||null,imageUrl:imageUrl||null,order:Number(f.elements.order.value),isVisible:f.elements.isVisible.checked,isFeatured:apps.find(a=>a.id===id)?.isFeatured??false};await api('/api/portal/apps'+(id?'/'+id:''),{method:id?'PUT':'POST',body:JSON.stringify(body)});$('#editor').close();await reloadAppsPreservingViewport(editorViewport);toast('Đã lưu ứng dụng.');
 }catch(error){$('#editor-error').textContent=error.message;}finally{b.disabled=false;}};
for(const b of document.querySelectorAll('dialog .close'))b.addEventListener('click',()=>b.closest('dialog').close());
$('#editor').addEventListener('close',()=>restoreViewport(editorViewport));
$('#delete-dialog').addEventListener('close',()=>restoreViewport(deleteViewport));
$('#about').onclick=()=>$('#about-dialog').showModal();
window.addEventListener('keydown',e=>{if(e.key==='/'&&!['INPUT','TEXTAREA','SELECT'].includes(document.activeElement.tagName)&&!document.querySelector('dialog[open]')&&!$('#portal-content').hidden){e.preventDefault();$('#search').focus();}});
async function restoreAdminSession(){
 const session=savedAdminSession;
 if(!session?.refreshToken||!isAdmin){adminSessionRestoring=false;adminState();return;}
 if(session.refreshTokenExpiresAt&&new Date(session.refreshTokenExpiresAt)<=new Date()){forgetAdminSession();adminState();return;}
 adminSessionRestoring=true;adminState();
 try{const response=await fetch('/api/auth/refresh',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({refreshToken:session.refreshToken,deviceName:'Gold Central · Trình duyệt'})});const result=await response.json().catch(()=>null);if(!response.ok||!result?.data?.accessToken||result.data.account.role!=='admin'){forgetAdminSession();adminState();return;}saveAdminSession(result.data);adminSessionRestoring=false;await switchView(adminViewFromUrl(),{syncHistory:false});}
 catch{adminSessionRestoring=false;adminState();$('#login-error').textContent='Chưa thể kết nối để khôi phục phiên. Hãy thử tải lại trang; bạn chưa cần đăng nhập lại.';}
}
adminState();loadMusicLibrary(musicDepartment);if(!isAdmin)load();else restoreAdminSession();


