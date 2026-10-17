'use strict';
const previewEscape=value=>String(value??'').replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
const previewDepartments={all:'Toàn công ty',vaccination:'Tiêm chủng',dermatology:'Da liễu & Thẩm mỹ',rehabilitation:'Phục hồi chức năng',wellness:'Dưỡng sinh',office:'Văn phòng & Nội bộ'};
const previewEye='<svg class="eye-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.7" aria-hidden="true"><path d="M2 12s3.6-6 10-6 10 6 10 6-3.6 6-10 6-10-6-10-6Z"/><circle cx="12" cy="12" r="2.6"/></svg>';
const articlePopup=document.createElement('dialog');articlePopup.className='news-preview-dialog';articlePopup.setAttribute('aria-label','Đọc tin tức');
articlePopup.innerHTML='<button type="button" class="preview-close" aria-label="Đóng bài viết" autofocus>×</button><div class="news-preview-content" aria-live="polite"></div>';document.body.append(articlePopup);
articlePopup.querySelector('button').onclick=()=>articlePopup.close();articlePopup.addEventListener('click',e=>{if(e.target===articlePopup)articlePopup.close();});
let previewVersion=0;articlePopup.addEventListener('close',()=>{previewVersion++;});
async function openNewsPreview(loader){
 const version=++previewVersion,content=articlePopup.querySelector('.news-preview-content');content.innerHTML='<div class="empty">Đang tải bài viết...</div>';if(!articlePopup.open)articlePopup.showModal();
 try{const a=await loader();if(version!==previewVersion||!articlePopup.open)return;document.querySelectorAll(`a.news-card[href="/news/${a.id}"] .news-view-count`).forEach(counter=>{counter.innerHTML=`${previewEye} ${(a.viewCount||0).toLocaleString('vi-VN')} lượt đọc`;});
 content.innerHTML=`<article class="news-article"><div class="news-meta"><span>${previewEscape(previewDepartments[a.department])}</span><time>${new Date(a.publishedAt||a.updatedAt).toLocaleDateString('vi-VN',{timeZone:'Asia/Ho_Chi_Minh'})}</time><span class="news-view-count">${previewEye} ${(a.viewCount||0).toLocaleString('vi-VN')} lượt đọc</span>${a.isPublished===false?'<span>Bản nháp / Đang ẩn</span>':''}</div><h1>${previewEscape(a.title)}</h1>${a.summary?`<p class="article-summary">${previewEscape(a.summary)}</p>`:''}${a.imageUrl?`<button type="button" class="news-image-zoom" aria-label="Xem ảnh phóng lớn"><img src="${previewEscape(a.imageUrl)}" alt="Ảnh bài viết"><span>⤢ Xem ảnh</span></button>`:''}<div class="article-content">${previewEscape(a.content)}</div></article>`;
 }catch(error){if(version===previewVersion&&articlePopup.open)content.innerHTML=`<div class="empty"><b>Chưa thể mở bài viết</b><p>${previewEscape(error.message)}</p></div>`;}
}
const imagePopup=document.createElement('dialog');imagePopup.className='news-image-dialog';imagePopup.setAttribute('aria-label','Ảnh phóng lớn');imagePopup.innerHTML='<button type="button" class="preview-close" aria-label="Đóng ảnh" autofocus>×</button><img alt="Ảnh bài viết phóng lớn"><p hidden>Không thể tải ảnh. Vui lòng thử lại sau.</p>';document.body.append(imagePopup);
const largeImage=imagePopup.querySelector('img');largeImage.onerror=()=>{largeImage.hidden=true;imagePopup.querySelector('p').hidden=false;};imagePopup.querySelector('button').onclick=()=>imagePopup.close();imagePopup.onclick=e=>{if(e.target===imagePopup)imagePopup.close();};imagePopup.addEventListener('close',()=>largeImage.removeAttribute('src'));
document.addEventListener('click',e=>{
 const image=e.target.closest('.news-article .news-cover img,.news-image-zoom');
 if(image){e.preventDefault();const img=image.matches('img')?image:image.querySelector('img');largeImage.hidden=false;imagePopup.querySelector('p').hidden=true;largeImage.src=img.src;imagePopup.showModal();return;}
 const card=e.target.closest('a.news-card');if(card&&!e.ctrlKey&&!e.metaKey&&!e.shiftKey&&!e.altKey){e.preventDefault();const id=card.getAttribute('href').split('/').pop();openNewsPreview(async()=>{const response=await fetch('/api/news/'+id);const data=await response.json();if(!response.ok)throw new Error(data?.message||'Bài viết không tồn tại hoặc đã được ẩn.');return data;});}
});
// Cover images on the standalone article also support keyboard activation.
function enhanceNewsImages(){document.querySelectorAll('.news-article .news-cover img').forEach(img=>{if(img.dataset.zoomReady)return;img.dataset.zoomReady='true';img.tabIndex=0;img.setAttribute('role','button');img.setAttribute('aria-label','Xem ảnh phóng lớn');img.onkeydown=e=>{if(e.key==='Enter'||e.key===' '){e.preventDefault();img.click();}};});}
new MutationObserver(enhanceNewsImages).observe(document.querySelector('#news-results')||articlePopup,{childList:true,subtree:true});enhanceNewsImages();
