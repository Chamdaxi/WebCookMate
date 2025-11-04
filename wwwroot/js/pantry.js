// Pantry Management - Categories & Ingredients

let allCategories = [];
let allIngredients = [];
let filteredIngredients = [];
let currentFilter = 'all';
let currentCategoryFilter = 'all';
let currentSort = 'name';
let editingCategoryId = null;
let editingIngredientId = null;

// Initialize on page load
document.addEventListener('DOMContentLoaded', async function() {
    console.log('🚀 Pantry page loaded, starting to load data...');
    await loadCategories();
    await loadIngredients();
    
    setupEventListeners();
});

// Setup Event Listeners
function setupEventListeners() {
    // Search
    document.getElementById('searchInput').addEventListener('input', handleSearch);
    
    // Filter buttons
    document.querySelectorAll('.filter-btn').forEach(btn => {
        btn.addEventListener('click', function() {
            document.querySelectorAll('.filter-btn').forEach(b => b.classList.remove('active'));
            this.classList.add('active');
            currentFilter = this.getAttribute('data-filter');
            applyFiltersAndSort();
        });
    });
    
    // Sort
    document.getElementById('sortSelect').addEventListener('change', function() {
        currentSort = this.value;
        applyFiltersAndSort();
    });
    
    // Modal triggers
    document.getElementById('addIngredientBtn').addEventListener('click', openAddIngredientModal);
    document.getElementById('manageCategoriesBtn').addEventListener('click', openCategoryModal);
    
    // Forms
    document.getElementById('ingredientForm').addEventListener('submit', handleIngredientSubmit);
    document.getElementById('categoryForm').addEventListener('submit', handleCategorySubmit);
    
    // Close modals on background click
    window.addEventListener('click', function(e) {
        if (e.target.classList.contains('modal')) {
            e.target.classList.remove('active');
            // Show header and navigation when modal closes
            const headerContainer = document.querySelector('.header-container');
            if (headerContainer) {
                headerContainer.style.display = '';
                headerContainer.style.visibility = 'visible';
            }
            const header = document.querySelector('.header');
            const navigation = document.querySelector('.navigation');
            if (header) header.style.display = '';
            if (navigation) navigation.style.display = '';
            // Restore body scroll
            document.body.style.overflow = '';
            // Reset forms
            if (e.target.id === 'categoryModal') {
                resetCategoryForm();
            } else if (e.target.id === 'ingredientModal') {
                resetIngredientForm();
            }
        }
    });
}

// ==================== CATEGORIES ====================

async function loadCategories() {
    try {
        console.log('🔄 Loading categories...');
        const response = await fetch('/api/IngredientCategoryApi', {
            credentials: 'include'  // Include cookies/session
        });
        
        console.log('📡 Response status:', response.status, response.statusText);
        
        if (!response.ok) {
            const errorText = await response.text();
            console.error('❌ Failed to load categories:', response.status, errorText);
            showNotification('Lỗi khi tải danh mục: ' + response.status, 'error');
            throw new Error('Failed to load categories: ' + response.status);
        }
        
        allCategories = await response.json();
        console.log('📦 Raw response:', allCategories);
        console.log('✅ Loaded categories from CookMate API:', allCategories.length);
        
        // Handle response format - might be wrapped in data property
        if (allCategories && typeof allCategories === 'object' && 'data' in allCategories) {
            allCategories = allCategories.data;
        }
        
        if (!Array.isArray(allCategories)) {
            console.error('Invalid categories format:', allCategories);
            allCategories = [];
        }
        
        renderCategoryTabs();
        renderCategoryList();
        updateStats();
        populateCategorySelect();
    } catch (error) {
        console.error('Error loading categories:', error);
        showNotification('Không thể tải danh mục: ' + error.message, 'error');
    }
}

function renderCategoryTabs() {
    const tabsContainer = document.getElementById('categoryTabs');
    
    const tabs = `
        <button class="category-tab ${currentCategoryFilter === 'all' ? 'active' : ''}" data-category-id="all" onclick="filterByCategory('all')">
            <i class="fas fa-globe"></i>
            <span>Tất Cả</span>
        </button>
        ${allCategories.map(cat => `
            <button class="category-tab ${currentCategoryFilter === cat.id ? 'active' : ''}" 
                    data-category-id="${cat.id}" 
                    onclick="filterByCategory('${cat.id}')">
                <span style="font-size: 1.2em;">${cat.icon || '📦'}</span>
                <span>${cat.name}</span>
            </button>
        `).join('')}
    `;
    
    tabsContainer.innerHTML = tabs;
}

function renderCategoryList() {
    const listContainer = document.getElementById('categoryList');
    
    if (allCategories.length === 0) {
        listContainer.innerHTML = '<p style="text-align: center; color: #999;">Chưa có danh mục nào</p>';
        return;
    }
    
    const html = allCategories.map(cat => `
        <div class="category-item">
            <div class="category-item-info">
                <div class="category-item-icon" style="background-color: ${cat.color || '#667eea'};">
                    <span style="font-size: 2em;">${cat.icon || '📦'}</span>
                </div>
                <div class="category-item-details">
                    <h4>${cat.name}</h4>
                    <p>${cat.description || 'Không có mô tả'}</p>
                </div>
            </div>
            <div class="category-item-actions">
                <button class="action-btn edit" onclick="editCategory('${cat.id}')" title="Sửa">
                    <i class="fas fa-edit"></i>
                </button>
                <button class="action-btn delete" onclick="deleteCategory('${cat.id}')" title="Xóa">
                    <i class="fas fa-trash"></i>
                </button>
            </div>
        </div>
    `).join('');
    
    listContainer.innerHTML = html;
}

function populateCategorySelect() {
    const select = document.getElementById('ingredientCategory');
    
    const options = allCategories.map(cat => 
        `<option value="${cat.id}">${cat.icon || '📦'} ${cat.name}</option>`
    ).join('');
    
    select.innerHTML = '<option value="">Chọn danh mục</option>' + options;
}

function openCategoryModal() {
    resetCategoryForm();
    document.getElementById('categoryModal').classList.add('active');
    // Hide header and navigation when modal opens
    const headerContainer = document.querySelector('.header-container');
    if (headerContainer) {
        headerContainer.style.display = 'none';
        headerContainer.style.visibility = 'hidden';
    }
    // Also hide header and navigation separately
    const header = document.querySelector('.header');
    const navigation = document.querySelector('.navigation');
    if (header) header.style.display = 'none';
    if (navigation) navigation.style.display = 'none';
    // Prevent body scroll when modal is open
    document.body.style.overflow = 'hidden';
}

function closeCategoryModal() {
    document.getElementById('categoryModal').classList.remove('active');
    resetCategoryForm();
    // Show header and navigation when modal closes
    const headerContainer = document.querySelector('.header-container');
    if (headerContainer) {
        headerContainer.style.display = '';
        headerContainer.style.visibility = 'visible';
    }
    const header = document.querySelector('.header');
    const navigation = document.querySelector('.navigation');
    if (header) header.style.display = '';
    if (navigation) navigation.style.display = '';
    // Restore body scroll
    document.body.style.overflow = '';
}

function resetCategoryForm() {
    editingCategoryId = null;
    document.getElementById('categoryForm').reset();
    document.getElementById('categoryId').value = '';
    document.getElementById('categoryFormTitle').textContent = 'Thêm Danh Mục Mới';
    document.getElementById('categorySubmitText').textContent = 'Thêm Danh Mục';
    document.getElementById('categoryColor').value = '#667eea';
}

async function handleCategorySubmit(e) {
    e.preventDefault();
    
    const categoryData = {
        id: document.getElementById('categoryId').value || undefined,
        name: document.getElementById('categoryName').value,
        description: document.getElementById('categoryDescription').value,
        icon: document.getElementById('categoryIcon').value,
        color: document.getElementById('categoryColor').value
    };
    
    try {
        let response;
        if (editingCategoryId) {
            categoryData.id = editingCategoryId;
            response = await fetch(`/api/IngredientCategoryApi/${editingCategoryId}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include',
                body: JSON.stringify(categoryData)
            });
        } else {
            response = await fetch('/api/IngredientCategoryApi', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include',
                body: JSON.stringify(categoryData)
            });
        }
        
        if (!response.ok) {
            const error = await response.text();
            throw new Error(error);
        }
        
        showNotification(editingCategoryId ? 'Đã cập nhật danh mục' : 'Đã thêm danh mục mới', 'success');
        await loadCategories();
        resetCategoryForm();
    } catch (error) {
        console.error('Error saving category:', error);
        showNotification('Lỗi: ' + error.message, 'error');
    }
}

async function editCategory(id) {
    const category = allCategories.find(c => c.id === id);
    if (!category) return;
    
    editingCategoryId = id;
    document.getElementById('categoryId').value = id;
    document.getElementById('categoryName').value = category.name;
    document.getElementById('categoryDescription').value = category.description || '';
    document.getElementById('categoryIcon').value = category.icon || '';
    document.getElementById('categoryColor').value = category.color || '#667eea';
    
    document.getElementById('categoryFormTitle').textContent = 'Sửa Danh Mục';
    document.getElementById('categorySubmitText').textContent = 'Cập Nhật';
}

async function deleteCategory(id) {
    const category = allCategories.find(c => c.id === id);
    if (!category) return;
    
    if (!confirm(`Bạn có chắc muốn xóa danh mục "${category.name}"?`)) return;
    
    try {
        const response = await fetch(`/api/IngredientCategoryApi/${id}`, {
            method: 'DELETE',
            credentials: 'include'
        });
        
        if (!response.ok) {
            const error = await response.text();
            throw new Error(error);
        }
        
        showNotification('Đã xóa danh mục', 'success');
        await loadCategories();
        await loadIngredients(); // Reload to update counts
    } catch (error) {
        console.error('Error deleting category:', error);
        showNotification('Lỗi: ' + error.message, 'error');
    }
}

// ==================== INGREDIENTS ====================

async function loadIngredients() {
    showLoading(true);
    try {
        console.log('🔄 Loading ingredients...');
        const response = await fetch('/api/IngredientApi', {
            credentials: 'include'  // Include cookies/session
        });
        
        console.log('📡 Response status:', response.status, response.statusText);
        
        if (!response.ok) {
            const errorText = await response.text();
            console.error('❌ Failed to load ingredients:', response.status, errorText);
            showNotification('Lỗi khi tải nguyên liệu: ' + response.status, 'error');
            throw new Error('Failed to load ingredients: ' + response.status);
        }
        
        allIngredients = await response.json();
        console.log('📦 Raw response:', allIngredients);
        console.log('✅ Loaded ingredients from CookMate API:', allIngredients.length);
        
        // Handle response format - might be wrapped in data property
        if (allIngredients && typeof allIngredients === 'object' && 'data' in allIngredients) {
            allIngredients = allIngredients.data;
        }
        
        if (!Array.isArray(allIngredients)) {
            console.error('Invalid ingredients format:', allIngredients);
            allIngredients = [];
        }
        
        applyFiltersAndSort();
        updateStats();
    } catch (error) {
        console.error('Error loading ingredients:', error);
        showNotification('Không thể tải nguyên liệu: ' + error.message, 'error');
    } finally {
        showLoading(false);
    }
}

function applyFiltersAndSort() {
    let ingredients = [...allIngredients];
    
    // Apply search filter
    const searchTerm = document.getElementById('searchInput').value.toLowerCase();
    if (searchTerm) {
        ingredients = ingredients.filter(ing => 
            ing.name.toLowerCase().includes(searchTerm) ||
            (ing.notes && ing.notes.toLowerCase().includes(searchTerm))
        );
    }
    
    // Apply category filter
    if (currentCategoryFilter !== 'all') {
        ingredients = ingredients.filter(ing => ing.categoryId === currentCategoryFilter);
    }
    
    // Apply expiring filter
    if (currentFilter === 'expiring') {
        const today = new Date();
        const weekFromNow = new Date();
        weekFromNow.setDate(today.getDate() + 7);
        
        ingredients = ingredients.filter(ing => {
            if (!ing.expiryDate) return false;
            const expiryDate = new Date(ing.expiryDate);
            return expiryDate <= weekFromNow;
        });
    }
    
    // Apply sorting
    ingredients.sort((a, b) => {
        switch (currentSort) {
            case 'name':
                return a.name.localeCompare(b.name);
            case 'name-desc':
                return b.name.localeCompare(a.name);
            case 'date':
                return new Date(b.createdAt) - new Date(a.createdAt);
            case 'expiry':
                if (!a.expiryDate) return 1;
                if (!b.expiryDate) return -1;
                return new Date(a.expiryDate) - new Date(b.expiryDate);
            case 'quantity':
                return b.quantity - a.quantity;
            default:
                return 0;
        }
    });
    
    filteredIngredients = ingredients;
    renderIngredients();
}

function renderIngredients() {
    const grid = document.getElementById('ingredientsGrid');
    const emptyState = document.getElementById('emptyState');
    
    if (filteredIngredients.length === 0) {
        grid.style.display = 'none';
        emptyState.style.display = 'flex';
        return;
    }
    
    grid.style.display = 'grid';
    emptyState.style.display = 'none';
    
    const html = filteredIngredients.map((ing, index) => {
        const category = allCategories.find(c => c.id === ing.categoryId);
        const expiryWarning = getExpiryWarning(ing.expiryDate);
        
        return `
            <div class="ingredient-card" style="animation-delay: ${index * 0.05}s;">
                <div class="ingredient-header">
                    <span class="ingredient-category-badge" style="background-color: ${category?.color || '#667eea'}20; color: ${category?.color || '#667eea'};">
                        <span>${category?.icon || '📦'}</span>
                        ${category?.name || 'Khác'}
                    </span>
                    <div class="ingredient-actions">
                        <button class="action-btn edit" onclick="editIngredient('${ing.id}')" title="Sửa">
                            <i class="fas fa-edit"></i>
                        </button>
                        <button class="action-btn delete" onclick="deleteIngredient('${ing.id}')" title="Xóa">
                            <i class="fas fa-trash"></i>
                        </button>
                    </div>
                </div>
                <h3 class="ingredient-name">${ing.name}</h3>
                <div class="ingredient-quantity">${ing.quantity} ${ing.unit}</div>
                ${expiryWarning ? `<div class="expiry-warning ${expiryWarning.class}"><i class="fas fa-exclamation-circle"></i> ${expiryWarning.text}</div>` : ''}
                <div class="ingredient-meta">
                    ${ing.expiryDate ? `<div class="meta-item"><i class="fas fa-calendar"></i> HSD: ${formatDate(ing.expiryDate)}</div>` : ''}
                    ${ing.notes ? `<div class="meta-item"><i class="fas fa-sticky-note"></i> ${ing.notes}</div>` : ''}
                    <div class="meta-item"><i class="fas fa-clock"></i> ${formatDate(ing.createdAt)}</div>
                </div>
            </div>
        `;
    }).join('');
    
    grid.innerHTML = html;
}

function openAddIngredientModal() {
    resetIngredientForm();
    document.getElementById('ingredientModal').classList.add('active');
    // Hide header and navigation when modal opens
    const headerContainer = document.querySelector('.header-container');
    if (headerContainer) {
        headerContainer.style.display = 'none';
        headerContainer.style.visibility = 'hidden';
    }
    // Also hide header and navigation separately
    const header = document.querySelector('.header');
    const navigation = document.querySelector('.navigation');
    if (header) header.style.display = 'none';
    if (navigation) navigation.style.display = 'none';
    // Prevent body scroll when modal is open
    document.body.style.overflow = 'hidden';
}

function closeIngredientModal() {
    document.getElementById('ingredientModal').classList.remove('active');
    resetIngredientForm();
    // Show header and navigation when modal closes
    const headerContainer = document.querySelector('.header-container');
    if (headerContainer) {
        headerContainer.style.display = '';
        headerContainer.style.visibility = 'visible';
    }
    const header = document.querySelector('.header');
    const navigation = document.querySelector('.navigation');
    if (header) header.style.display = '';
    if (navigation) navigation.style.display = '';
    // Restore body scroll
    document.body.style.overflow = '';
}

function resetIngredientForm() {
    editingIngredientId = null;
    document.getElementById('ingredientForm').reset();
    document.getElementById('ingredientId').value = '';
    document.getElementById('ingredientModalTitle').textContent = 'Thêm Nguyên Liệu';
    document.getElementById('ingredientSubmitText').textContent = 'Thêm Nguyên Liệu';
}

async function handleIngredientSubmit(e) {
    e.preventDefault();
    
    const ingredientData = {
        id: document.getElementById('ingredientId').value || undefined,
        name: document.getElementById('ingredientName').value,
        categoryId: document.getElementById('ingredientCategory').value,
        quantity: parseFloat(document.getElementById('ingredientQuantity').value),
        unit: document.getElementById('ingredientUnit').value,
        expiryDate: document.getElementById('ingredientExpiry').value || null,
        notes: document.getElementById('ingredientNotes').value || null,
        imageUrl: document.getElementById('ingredientImage').value || null
    };
    
    try {
        let response;
        if (editingIngredientId) {
            ingredientData.id = editingIngredientId;
            response = await fetch(`/api/IngredientApi/${editingIngredientId}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include',
                body: JSON.stringify(ingredientData)
            });
        } else {
            response = await fetch('/api/IngredientApi', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include',
                body: JSON.stringify(ingredientData)
            });
        }
        
        if (!response.ok) {
            const error = await response.text();
            throw new Error(error);
        }
        
        showNotification(editingIngredientId ? 'Đã cập nhật nguyên liệu' : 'Đã thêm nguyên liệu mới', 'success');
        await loadIngredients();
        closeIngredientModal();
    } catch (error) {
        console.error('Error saving ingredient:', error);
        showNotification('Lỗi: ' + error.message, 'error');
    }
}

async function editIngredient(id) {
    const ingredient = allIngredients.find(i => i.id === id);
    if (!ingredient) return;
    
    editingIngredientId = id;
    document.getElementById('ingredientId').value = id;
    document.getElementById('ingredientName').value = ingredient.name;
    document.getElementById('ingredientCategory').value = ingredient.categoryId;
    document.getElementById('ingredientQuantity').value = ingredient.quantity;
    document.getElementById('ingredientUnit').value = ingredient.unit;
    document.getElementById('ingredientExpiry').value = ingredient.expiryDate ? ingredient.expiryDate.split('T')[0] : '';
    document.getElementById('ingredientNotes').value = ingredient.notes || '';
    document.getElementById('ingredientImage').value = ingredient.imageUrl || '';
    
    document.getElementById('ingredientModalTitle').textContent = 'Sửa Nguyên Liệu';
    document.getElementById('ingredientSubmitText').textContent = 'Cập Nhật';
    
    document.getElementById('ingredientModal').classList.add('active');
}

async function deleteIngredient(id) {
    const ingredient = allIngredients.find(i => i.id === id);
    if (!ingredient) return;
    
    if (!confirm(`Bạn có chắc muốn xóa "${ingredient.name}"?`)) return;
    
    try {
        const response = await fetch(`/api/IngredientApi/${id}`, {
            method: 'DELETE',
            credentials: 'include'
        });
        
        if (!response.ok) {
            const error = await response.text();
            throw new Error(error);
        }
        
        showNotification('Đã xóa nguyên liệu', 'success');
        await loadIngredients();
    } catch (error) {
        console.error('Error deleting ingredient:', error);
        showNotification('Lỗi: ' + error.message, 'error');
    }
}

// ==================== HELPERS ====================

function filterByCategory(categoryId) {
    currentCategoryFilter = categoryId;
    
    // Update active tab
    document.querySelectorAll('.category-tab').forEach(tab => {
        tab.classList.toggle('active', tab.getAttribute('data-category-id') === categoryId);
    });
    
    applyFiltersAndSort();
}

function handleSearch() {
    applyFiltersAndSort();
}

function updateStats() {
    document.getElementById('totalCategories').textContent = allCategories.length;
    document.getElementById('totalIngredients').textContent = allIngredients.length;
    
    // Count expiring items (within 7 days)
    const today = new Date();
    const weekFromNow = new Date();
    weekFromNow.setDate(today.getDate() + 7);
    
    const expiringCount = allIngredients.filter(ing => {
        if (!ing.expiryDate) return false;
        const expiryDate = new Date(ing.expiryDate);
        return expiryDate <= weekFromNow;
    }).length;
    
    document.getElementById('expiringItems').textContent = expiringCount;
}

function getExpiryWarning(expiryDate) {
    if (!expiryDate) return null;
    
    const today = new Date();
    const expiry = new Date(expiryDate);
    const daysUntilExpiry = Math.ceil((expiry - today) / (1000 * 60 * 60 * 24));
    
    if (daysUntilExpiry < 0) {
        return { text: 'Đã hết hạn', class: 'expiry-danger' };
    } else if (daysUntilExpiry === 0) {
        return { text: 'Hết hạn hôm nay!', class: 'expiry-danger' };
    } else if (daysUntilExpiry <= 3) {
        return { text: `Hết hạn trong ${daysUntilExpiry} ngày`, class: 'expiry-danger' };
    } else if (daysUntilExpiry <= 7) {
        return { text: `Hết hạn trong ${daysUntilExpiry} ngày`, class: '' };
    }
    
    return null;
}

function formatDate(dateString) {
    const date = new Date(dateString);
    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const year = date.getFullYear();
    return `${day}/${month}/${year}`;
}

function showLoading(show) {
    document.getElementById('loadingSpinner').style.display = show ? 'flex' : 'none';
}

function showNotification(message, type = 'success') {
    const notification = document.getElementById('notification');
    const messageEl = document.getElementById('notificationMessage');
    
    messageEl.textContent = message;
    notification.classList.remove('error');
    
    if (type === 'error') {
        notification.classList.add('error');
    }
    
    notification.classList.add('show');
    
    setTimeout(() => {
        notification.classList.remove('show');
    }, 3000);
}

