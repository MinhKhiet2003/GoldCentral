'use strict';
let newsAdminPage=1,newsAdminVersion=0,newsAdminData=null,newsEditorPosition=null,newsDeleteId=null,newsDeletePosition=null;
const newsDepartments={all:'Toàn công ty',...categories};
const newsDate=value=>new Date(value).toLocaleDateString('vi-VN',{timeZone:'Asia/Ho_Chi_Minh'});
const newsEye='<svg class="eye-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.7" aria-hidden="true"><path d="M2 12s3.6-6 10-6 10 6 10 6-3.6 6-10 6-10-6-10-6Z"/><circle cx="12" cy="12" r="2.6"/></svg>';
async function loadNewsAdmin(preserve=false){
 if(!isAdmin||!token||view!=='news')return;const version=++newsAdminVersion,position=preserve?captureViewport():null;
 if(!preserve)$('#news-admin-results').innerHTML='<div class="empty">Đang tải bài viết...</div>';
 const query=new URLSearchParams({page:newsAdminPage,size:12,keyword:$('#news-admin-search').value.trim()});if($('#news-admin-status').value)query.set('published',$('#news-admin-status').value);
 try{const data=await api('/api/news/manage?'+query);if(version!==newsAdminVersion||view!=='news'||!token)return;if(newsAdminPage>1&&!data.items.length){newsAdminPage--;return loadNewsAdmin(preserve);}newsAdminData=data;renderNewsAdmin();restoreViewport(position);}catch(e){if(version===newsAdminVersion)$('#news-admin-results').innerHTML=`<div class="empty"><b>Chưa thể tải tin tức</b><p>${esc(e.message)}</p><button class="secondary" data-news-action="retry">Thử lại</button></div>`;}
}
function renderNewsAdmin(){
 const data=newsAdminData;$('#news-admin-results').innerHTML=data.items.length?`<div class="news-table-wrap"><table class="news-table"><thead><tr><th>BÀI VIẾT</th><th>PHÒNG BAN</th><th>LƯỢT ĐỌC</th><th>TRẠNG THÁI</th><th>CẬP NHẬT</th><th>THAO TÁC</th></tr></thead><tbody>${data.items.map(a=>`<tr data-news-read="${a.id}" tabindex="0" aria-label="Đọc ${esc(a.title)}"><td><b>${esc(a.title)}</b><p>${esc(a.summary)}</p></td><td>${esc(newsDepartments[a.department])}</td><td><span class="news-view-count">${newsEye} ${(a.viewCount||0).toLocaleString('vi-VN')} lượt</span></td><td><span class="news-status ${a.isPublished?'published':''}">${a.isPublished?'Đã xuất bản':'Bản nháp / Ẩn'}</span></td><td>${newsDate(a.updatedAt)}</td><td><div class="news-row-actions"><button data-news-action="edit" data-id="${a.id}">Sửa</button><button data-news-action="toggle" data-id="${a.id}">${a.isPublished?'Ẩn bài':'Xuất bản'}</button><button class="delete" data-news-action="delete" data-id="${a.id}">Xóa</button></div></td></tr>`).join('')}</tbody></table></div>`:'<div class="empty"><b>Chưa có bài viết phù hợp</b><p>Viết bài mới hoặc thay đổi bộ lọc để xem thêm.</p></div>';
 $('#news-admin-pagination').innerHTML=`<span>${data.totalItems} bài viết · Trang ${data.pageNumber} / ${Math.max(1,data.totalPages)}</span><div><button class="secondary" data-news-page="prev" ${data.pageNumber<=1?'disabled':''}>← Trước</button><button class="secondary" data-news-page="next" ${data.pageNumber>=data.totalPages?'disabled':''}>Sau →</button></div>`;
}
async function editNews(id){
 newsEditorPosition=captureViewport();const f=$('#news-form');f.reset();$('#news-editor-error').textContent='';f.elements.id.value=id||'';
 try{if(id){const a=await api('/api/news/manage/'+id);for(const key of ['title','summary','content','imageUrl','department'])f.elements[key].value=a[key]??'';f.elements.isPublished.checked=a.isPublished;}$('#news-editor-title').textContent=id?'Chỉnh sửa bài viết':'Viết bài mới';$('#news-editor').showModal();}catch(e){toast(e.message);}
}
$('#add-news').onclick=()=>editNews();
let newsSearchTimer;$('#news-admin-search').oninput=()=>{clearTimeout(newsSearchTimer);newsSearchTimer=setTimeout(()=>{newsAdminPage=1;loadNewsAdmin(true);},300);};$('#news-admin-status').onchange=()=>{newsAdminPage=1;loadNewsAdmin(true);};
$('#news-admin-pagination').onclick=e=>{const b=e.target.closest('[data-news-page]');if(!b||b.disabled)return;newsAdminPage+=b.dataset.newsPage==='next'?1:-1;loadNewsAdmin(true);};
$('#news-admin-results').onclick=async e=>{
 const b=e.target.closest('[data-news-action]');if(!b){const row=e.target.closest('[data-news-read]');if(row)openNewsPreview(()=>api('/api/news/manage/'+row.dataset.newsRead));return;}const id=b.dataset.id;
 if(b.dataset.newsAction==='retry')return loadNewsAdmin();if(b.dataset.newsAction==='edit')return editNews(id);
 if(b.dataset.newsAction==='delete'){newsDeleteId=id;newsDeletePosition=captureViewport();$('#news-delete-title').textContent=newsAdminData.items.find(a=>a.id===id)?.title||'';$('#news-delete-error').textContent='';$('#news-delete-dialog').showModal();return;}
 if(b.dataset.newsAction==='toggle'){b.disabled=true;try{const a=await api('/api/news/manage/'+id);await api('/api/news/'+id,{method:'PUT',body:JSON.stringify({...a,isPublished:!a.isPublished})});await loadNewsAdmin(true);toast('Đã cập nhật trạng thái bài viết.');}catch(e){toast(e.message);}finally{b.disabled=false;}}
};
$('#news-form').onsubmit=async e=>{e.preventDefault();const f=e.target,b=f.querySelector('[type=submit]');b.disabled=true;$('#news-editor-error').textContent='';try{
 let image=f.elements.imageUrl.value||null;const file=$('#news-image-file').files[0];if(file){if(file.size>10*1024*1024||!['image/png','image/jpeg'].includes(file.type))throw new Error('Ảnh PNG/JPG tối đa 10 MB.');const form=new FormData();form.append('file',file);image=(await api('/api/files/upload',{method:'POST',body:form})).data.url;}
 const id=f.elements.id.value,body={title:f.elements.title.value,summary:f.elements.summary.value,content:f.elements.content.value,department:f.elements.department.value,imageUrl:image,isPublished:f.elements.isPublished.checked};await api('/api/news'+(id?'/'+id:''),{method:id?'PUT':'POST',body:JSON.stringify(body)});$('#news-editor').close();await loadNewsAdmin(true);restoreViewport(newsEditorPosition);toast('Đã lưu bài viết.');
 }catch(error){$('#news-editor-error').textContent=error.message;}finally{b.disabled=false;}};
$('#confirm-delete-news').onclick=async()=>{const b=$('#confirm-delete-news');b.disabled=true;try{await api('/api/news/'+newsDeleteId,{method:'DELETE'});$('#news-delete-dialog').close();await loadNewsAdmin(true);restoreViewport(newsDeletePosition);toast('Đã xóa bài viết.');}catch(e){$('#news-delete-error').textContent=e.message;}finally{b.disabled=false;}};
$('#news-editor').addEventListener('close',()=>restoreViewport(newsEditorPosition));$('#news-delete-dialog').addEventListener('close',()=>restoreViewport(newsDeletePosition));

$('#news-admin-results').addEventListener('keydown',e=>{if(e.target.matches('[data-news-read]')&&['Enter',' '].includes(e.key)){e.preventDefault();e.target.click();}});
