'use strict';
const n$=s=>document.querySelector(s),articleId=document.body.dataset.articleId;
const nEsc=s=>String(s??'').replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
const nDepartments={all:'Toàn công ty',vaccination:'Tiêm chủng',dermatology:'Da liễu & Thẩm mỹ',general:'Khám chữa bệnh',office:'Văn phòng & Nội bộ'};
const nDate=value=>new Date(value).toLocaleDateString('vi-VN',{timeZone:'Asia/Ho_Chi_Minh'});
const nEye='<svg class="eye-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.7" aria-hidden="true"><path d="M2 12s3.6-6 10-6 10 6 10 6-3.6 6-10 6-10-6-10-6Z"/><circle cx="12" cy="12" r="2.6"/></svg>';
let newsPage=1,newsRequest=0;
async function newsApi(url){
 const r=await fetch(url),data=await r.json().catch(()=>null);
 if(!r.ok)throw new Error(data?.message||(r.status===404?'Bài viết không tồn tại hoặc đã được ẩn.':'Không thể tải tin tức. Vui lòng thử lại.'));return data;
}
function newsCover(a){return a.imageUrl?`<div class="news-cover"><img src="${nEsc(a.imageUrl)}" alt="" loading="lazy"></div>`:'<div class="news-cover news-cover-default"><span aria-hidden="true">✚</span><span>GOLD · KẾT NỐI NỘI BỘ</span></div>';}
async function loadNewsReader(){
 const version=++newsRequest;n$('#news-results').innerHTML='<div class="empty">Đang tải tin tức...</div>';
 try{
 if(articleId){const a=await newsApi('/api/news/'+articleId);if(version!==newsRequest)return;document.title=a.title+' · Gold';n$('#news-results').innerHTML=`<article class="news-article"><a class="back-home" href="/news">← Tất cả tin tức</a><div class="news-meta"><span>${nEsc(nDepartments[a.department])}</span><time>${nDate(a.publishedAt)}</time><span class="news-view-count">${nEye} ${(a.viewCount||0).toLocaleString('vi-VN')} lượt đọc</span></div><h1>${nEsc(a.title)}</h1>${a.summary?`<p class="article-summary">${nEsc(a.summary)}</p>`:''}${a.imageUrl?newsCover(a):''}<div class="article-content">${nEsc(a.content)}</div></article>`;n$('#news-pagination').replaceChildren();}
 else{const query=new URLSearchParams({page:newsPage,size:9,keyword:n$('#news-search').value.trim(),department:n$('#news-department').value});const data=await newsApi('/api/news?'+query);if(version!==newsRequest)return;n$('#news-results').innerHTML=data.items.length?`<div class="news-grid">${data.items.map(a=>`<a class="news-card" href="/news/${a.id}">${newsCover(a)}<div class="news-card-content"><div class="news-meta"><span>${nEsc(nDepartments[a.department])}</span><time>${nDate(a.publishedAt)}</time></div><h2>${nEsc(a.title)}</h2><p>${nEsc(a.summary)}</p><span class="news-view-count">${nEye} ${(a.viewCount||0).toLocaleString('vi-VN')} lượt đọc</span><span class="news-read-more">Đọc bài viết ↗</span></div></a>`).join('')}</div>`:'<div class="empty"><b>Chưa có tin tức phù hợp</b><p>Bài viết mới sẽ được cập nhật tại đây.</p></div>';n$('#news-pagination').innerHTML=`<span>${data.totalItems} bài viết · Trang ${data.pageNumber} / ${Math.max(1,data.totalPages)}</span><div><button class="secondary" data-page="prev" ${data.pageNumber<=1?'disabled':''}>← Trước</button><button class="secondary" data-page="next" ${data.pageNumber>=data.totalPages?'disabled':''}>Sau →</button></div>`;}
 n$('#news-results').querySelectorAll('.news-cover img').forEach(img=>img.addEventListener('error',()=>img.remove(),{once:true}));
 }catch(e){if(version===newsRequest)n$('#news-results').innerHTML=`<div class="empty"><b>Chưa thể mở tin tức</b><p>${nEsc(e.message)}</p><button class="secondary" id="retry-news">Thử lại</button></div>`;}
}
let readerSearchTimer;n$('#news-search').oninput=()=>{clearTimeout(readerSearchTimer);readerSearchTimer=setTimeout(()=>{newsPage=1;loadNewsReader();},300);};n$('#news-department').onchange=()=>{newsPage=1;loadNewsReader();};
n$('#news-results').onclick=e=>{if(e.target.closest('#retry-news'))loadNewsReader();};
n$('#news-pagination').onclick=e=>{const b=e.target.closest('[data-page]');if(!b||b.disabled)return;newsPage+=b.dataset.page==='next'?1:-1;loadNewsReader();};
n$('#news-list-tools').hidden=Boolean(articleId);
loadNewsReader();
