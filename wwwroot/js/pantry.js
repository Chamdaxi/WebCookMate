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
        
        const rawResponse = await response.json();
        console.log('📦 Raw response:', rawResponse);
        console.log('📦 Raw response type:', typeof rawResponse);
        console.log('📦 Raw response is array:', Array.isArray(rawResponse));
        
        // Handle response format - might be wrapped in data property
        let categoriesData = rawResponse;
        if (categoriesData && typeof categoriesData === 'object' && 'data' in categoriesData) {
            categoriesData = categoriesData.data;
        }
        
        if (!Array.isArray(categoriesData)) {
            console.error('❌ Invalid categories format:', categoriesData);
            allCategories = [];
        } else {
            // Normalize category objects - handle both PascalCase and camelCase
            allCategories = categoriesData.map((cat, index) => {
                // Log first category to see structure
                if (index === 0) {
                    console.log('🔍 First category raw object:', cat);
                    console.log('🔍 First category keys:', Object.keys(cat));
                    console.log('🔍 First category values:', Object.entries(cat).map(([k, v]) => `${k}: ${v}`).join(', '));
                }
                
                // Try ALL possible ID field names (case-insensitive check)
                let id = '';
                const keys = Object.keys(cat);
                
                // Check for ID in various forms
                for (const key of keys) {
                    const keyLower = key.toLowerCase();
                    if ((keyLower === 'id' || keyLower === '_id' || keyLower.includes('categoryid')) && cat[key]) {
                        id = String(cat[key]);
                        break;
                    }
                }
                
                // If still no ID, try direct property access (case-insensitive)
                if (!id) {
                    for (const key of keys) {
                        const value = cat[key];
                        if (value && typeof value === 'string' && value.length > 10) {
                            // MongoDB ObjectId is 24 chars, could be ID
                            if (key.toLowerCase().includes('id')) {
                                id = String(value);
                                console.log(`🔍 Found potential ID in key '${key}': ${id}`);
                                break;
                            }
                        }
                    }
                }
                
                // Fallback: try common field names directly
                if (!id) {
                    id = cat.id || cat.Id || cat._id || cat.ID || 
                         cat.categoryId || cat.CategoryId || cat.categoryID ||
                         cat.ingredientCategoryId || cat.IngredientCategoryId || 
                         cat.ingredientCategoryID || '';
                }
                
                const name = cat.name || cat.Name || '';
                const icon = cat.icon || cat.Icon || '📦';
                const userId = cat.userId || cat.UserId || '';
                
                // Log if ID is missing with full details
                if (!id || id === '') {
                    console.error(`❌ Category ${index} has no ID:`, {
                        name: name,
                        allKeys: keys,
                        allValues: Object.entries(cat).reduce((acc, [k, v]) => {
                            acc[k] = typeof v === 'string' ? v.substring(0, 50) : v;
                            return acc;
                        }, {})
                    });
                } else {
                    console.log(`✅ Category ${index} '${name}' has ID: ${id}`);
                }
                
                // Return normalized object with lowercase keys
                return {
                    id: id,
                    name: name,
                    icon: icon,
                    userId: userId,
                    // Keep original for reference
                    _original: cat
                };
            });
            
            console.log('✅ Normalized categories:', allCategories.map(c => ({ id: c.id, name: c.name })));
            console.log('✅ Loaded categories from CookMate API:', allCategories.length);
            
            // Count categories with valid ID
            const validCount = allCategories.filter(c => c.id && c.id !== '').length;
            console.log(`✅ Categories with valid ID: ${validCount}/${allCategories.length}`);
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
    
    // Filter valid categories with ID
    const validCategories = allCategories.filter(cat => cat && cat.id && cat.id !== '');
    
    const tabs = `
        <button class="category-tab ${currentCategoryFilter === 'all' ? 'active' : ''}" data-category-id="all" onclick="filterByCategory('all')">
            <i class="fas fa-globe"></i>
            <span>Tất Cả</span>
        </button>
        ${validCategories.map(cat => `
            <button class="category-tab ${currentCategoryFilter === cat.id ? 'active' : ''}" 
                    data-category-id="${cat.id}" 
                    onclick="filterByCategory('${cat.id}')">
                <span style="font-size: 1.2em;">${cat.icon || '📦'}</span>
                <span>${cat.name || 'Unknown'}</span>
            </button>
        `).join('')}
    `;
    
    tabsContainer.innerHTML = tabs;
}

function renderCategoryList() {
    const listContainer = document.getElementById('categoryList');
    
    // Filter valid categories with ID
    const validCategories = allCategories.filter(cat => cat && cat.id && cat.id !== '');
    
    if (validCategories.length === 0) {
        listContainer.innerHTML = '<p style="text-align: center; color: #999;">Chưa có danh mục nào</p>';
        return;
    }
    
    const html = validCategories.map(cat => `
        <div class="category-item">
            <div class="category-item-info">
                <div class="category-item-icon" style="background-color: ${cat.color || '#667eea'};">
                    <span style="font-size: 2em;">${cat.icon || '📦'}</span>
                </div>
                <div class="category-item-details">
                    <h4>${cat.name || 'Unknown'}</h4>
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
    
    if (!select) {
        console.warn('⚠️ Category select element not found');
        return;
    }
    
    if (!allCategories || allCategories.length === 0) {
        console.warn('⚠️ No categories available to populate');
        select.innerHTML = '<option value="">Chưa có danh mục nào</option>';
        return;
    }
    
    // Debug: Log first category to see structure
    if (allCategories.length > 0) {
        console.log('🔍 First category object (normalized):', allCategories[0]);
        console.log('🔍 First category ID:', allCategories[0].id);
    }
    
    // Filter out categories without ID
    const validCategories = allCategories.filter(cat => {
        if (!cat.id || cat.id === '') {
            console.warn('⚠️ Skipping category without ID:', cat);
            return false;
        }
        return true;
    });
    
    if (validCategories.length === 0) {
        console.error('❌ No valid categories with ID found!');
        select.innerHTML = '<option value="">Không có danh mục hợp lệ</option>';
        return;
    }
    
    const options = validCategories.map((cat, index) => {
        const catId = cat.id; // Already normalized, should have id
        const catName = cat.name || 'Unknown';
        const catIcon = cat.icon || '📦';
        
        // Double check ID is not empty
        if (!catId || catId === '') {
            console.error(`❌ Category ${index} still has no ID after normalization:`, cat);
            return null;
        }
        
        return `<option value="${catId}">${catIcon} ${catName}</option>`;
    }).filter(opt => opt !== null).join('');
    
    select.innerHTML = '<option value="">Chọn danh mục</option>' + options;
    console.log(`✅ Populated ${validCategories.length} categories in select`);
    
    // Verify populated options - check that all have non-empty values
    const optionsArray = Array.from(select.options);
    console.log('🔍 Select options verification:', optionsArray.map(opt => ({ value: opt.value, text: opt.text, hasValue: !!opt.value && opt.value !== '' })));
    
    // Check if any option has empty value (except the first one)
    const invalidOptions = optionsArray.slice(1).filter(opt => !opt.value || opt.value === '');
    if (invalidOptions.length > 0) {
        console.error('❌ Found options with empty values:', invalidOptions);
    }
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
    
    const name = document.getElementById('categoryName').value.trim();
    const icon = document.getElementById('categoryIcon').value.trim();
    
    if (!name || !icon) {
        showNotification('Vui lòng điền đầy đủ tên và icon', 'error');
        return;
    }
    
    const categoryData = {
        name: name,
        icon: icon
    };
    
    try {
        let response;
        if (editingCategoryId) {
            // Update: need to send categoryId in body
            categoryData.categoryId = editingCategoryId;
            response = await fetch('/api/IngredientCategoryApi', {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include',
                body: JSON.stringify(categoryData)
            });
        } else {
            // Create
            response = await fetch('/api/IngredientCategoryApi', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include',
                body: JSON.stringify(categoryData)
            });
        }
        
        if (!response.ok) {
            const errorText = await response.text();
            let errorMessage = 'Lỗi khi lưu danh mục';
            try {
                const errorJson = JSON.parse(errorText);
                errorMessage = errorJson.message || errorJson.error || errorMessage;
            } catch {
                errorMessage = errorText || errorMessage;
            }
            throw new Error(errorMessage);
        }
        
        showNotification(editingCategoryId ? 'Đã cập nhật danh mục' : 'Đã thêm danh mục mới', 'success');
        await loadCategories();
        closeCategoryModal();
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
    if (!id) {
        console.error('❌ Delete category called with empty ID');
        showNotification('Lỗi: Không có ID danh mục', 'error');
        return;
    }
    
    const category = allCategories.find(c => c.id === id);
    if (!category) {
        console.error('❌ Category not found:', id);
        showNotification('Không tìm thấy danh mục', 'error');
        return;
    }
    
    if (!confirm(`Bạn có chắc muốn xóa danh mục "${category.name}"?`)) return;
    
    try {
        console.log('🗑️ Deleting category:', id);
        const response = await fetch(`/api/IngredientCategoryApi/${id}`, {
            method: 'DELETE',
            credentials: 'include',
            headers: {
                'Content-Type': 'application/json'
            }
        });
        
        const responseText = await response.text();
        console.log('📥 Delete response:', response.status, responseText);
        
        if (!response.ok) {
            let errorMessage = 'Không thể xóa danh mục';
            try {
                const errorJson = JSON.parse(responseText);
                errorMessage = errorJson.message || errorJson.error || errorMessage;
            } catch (e) {
                errorMessage = responseText || errorMessage;
            }
            throw new Error(errorMessage);
        }
        
        showNotification('Đã xóa danh mục thành công', 'success');
        await loadCategories();
        await loadIngredients(); // Reload to update counts
    } catch (error) {
        console.error('❌ Error deleting category:', error);
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
        
        const responseData = await response.json();
        console.log('📦 Raw response:', responseData);
        console.log('📦 Response type:', typeof responseData, Array.isArray(responseData));
        
        // Handle different response formats
        if (Array.isArray(responseData)) {
            allIngredients = responseData;
        } else if (responseData && typeof responseData === 'object') {
            // Try common property names
            if ('data' in responseData && Array.isArray(responseData.data)) {
                allIngredients = responseData.data;
            } else if ('ingredients' in responseData && Array.isArray(responseData.ingredients)) {
                allIngredients = responseData.ingredients;
            } else if ('items' in responseData && Array.isArray(responseData.items)) {
                allIngredients = responseData.items;
            } else if ('results' in responseData && Array.isArray(responseData.results)) {
                allIngredients = responseData.results;
            } else {
                console.error('❌ Invalid ingredients format - not an array and no known array property:', responseData);
                allIngredients = [];
            }
        } else {
            console.error('❌ Invalid ingredients format - not an object or array:', responseData);
            allIngredients = [];
        }
        
        console.log('✅ Loaded ingredients from CookMate API:', allIngredients.length);
        
        // Normalize field names (handle both camelCase and PascalCase)
        // IMPORTANT: Modify the object in place to preserve all fields
        allIngredients = allIngredients.map(ing => {
            // Normalize ID field (priority: id > Id > _id > ingredientId)
            if (!ing.id) {
                ing.id = ing.Id || ing._id || ing.ingredientId || '';
            }
            
            // Normalize name field
            if (!ing.name) {
                ing.name = ing.Name || 'Unknown';
            }
            
            // Normalize categoryId field
            if (!ing.categoryId) {
                ing.categoryId = ing.CategoryId || ing.categoryID || '';
            }
            
            // Normalize quantity field
            if (ing.quantity === undefined && ing.Quantity !== undefined) {
                ing.quantity = ing.Quantity;
            } else if (ing.quantity === undefined) {
                ing.quantity = 0;
            }
            
            // Normalize unit field
            if (!ing.unit) {
                ing.unit = ing.Unit || 'piece';
            }
            
            // Normalize expiryDate field (priority: expiryDate > ExpireDate > expireDate)
            if (!ing.expiryDate) {
                ing.expiryDate = ing.ExpireDate || ing.expireDate || null;
            }
            
            // Normalize notes field
            if (!ing.notes) {
                ing.notes = ing.Notes || '';
            }
            
            // Normalize createdAt field
            if (!ing.createdAt) {
                ing.createdAt = ing.CreatedAt || ing.created_at || null;
            }
            
            // Ensure id is not empty (critical for rendering)
            if (!ing.id && ing._id) {
                ing.id = ing._id;
            }
            
            return ing;
        });
        
        // Filter out ingredients without ID (they cannot be rendered)
        const beforeFilter = allIngredients.length;
        allIngredients = allIngredients.filter(ing => {
            if (!ing.id || ing.id === '') {
                console.warn('⚠️ Filtering out ingredient without ID:', ing.name || 'Unknown');
                return false;
            }
            return true;
        });
        if (allIngredients.length < beforeFilter) {
            console.warn(`⚠️ Filtered out ${beforeFilter - allIngredients.length} ingredients without ID`);
        }
        
        // Log normalized ingredients for debugging
        if (allIngredients.length > 0) {
            console.log('🔍 First normalized ingredient:', {
                id: allIngredients[0].id,
                name: allIngredients[0].name,
                categoryId: allIngredients[0].categoryId
            });
        }
        
        console.log(`✅ Normalized ${allIngredients.length} ingredients`);
        console.log(`🔍 Sample ingredient:`, allIngredients.length > 0 ? allIngredients[0] : 'none');
        
        applyFiltersAndSort();
        updateStats();
        
        console.log(`📊 After filtering: ${filteredIngredients.length} ingredients to display`);
    } catch (error) {
        console.error('Error loading ingredients:', error);
        showNotification('Không thể tải nguyên liệu: ' + error.message, 'error');
        allIngredients = [];
        filteredIngredients = [];
        renderIngredients();
        updateStats();
    } finally {
        showLoading(false);
    }
}

function applyFiltersAndSort() {
    console.log(`🔄 applyFiltersAndSort START:`);
    console.log(`   - allIngredients: ${allIngredients.length}`);
    console.log(`   - currentCategoryFilter: "${currentCategoryFilter}"`);
    console.log(`   - currentFilter: "${currentFilter}"`);
    console.log(`   - currentSort: "${currentSort}"`);
    
    // Ensure we're working with a copy
    let ingredients = allIngredients.length > 0 ? [...allIngredients] : [];
    console.log(`📋 Starting with ${ingredients.length} ingredients`);
    
    if (ingredients.length === 0) {
        console.warn('⚠️ No ingredients to filter!');
        filteredIngredients = [];
        renderIngredients();
        return;
    }
    
    // Apply search filter
    const searchInput = document.getElementById('searchInput');
    const searchTerm = searchInput ? searchInput.value.toLowerCase() : '';
    if (searchTerm) {
        const beforeSearch = ingredients.length;
        ingredients = ingredients.filter(ing => {
            const nameMatch = ing.name && ing.name.toLowerCase().includes(searchTerm);
            const notesMatch = ing.notes && ing.notes.toLowerCase().includes(searchTerm);
            return nameMatch || notesMatch;
        });
        console.log(`🔍 After search filter: ${beforeSearch} -> ${ingredients.length}`);
    }
    
    // Apply category filter
    if (currentCategoryFilter !== 'all') {
        const beforeCategory = ingredients.length;
        ingredients = ingredients.filter(ing => {
            // Normalize categoryId for comparison
            const ingCategoryId = ing.categoryId || ing.CategoryId || '';
            const match = ingCategoryId === currentCategoryFilter;
            if (!match && beforeCategory < 10) {
                console.log(`❌ Ingredient "${ing.name || 'Unknown'}" (categoryId: "${ingCategoryId}") doesn't match filter "${currentCategoryFilter}"`);
            }
            return match;
        });
        console.log(`🏷️ After category filter "${currentCategoryFilter}": ${beforeCategory} -> ${ingredients.length}`);
        if (ingredients.length === 0 && beforeCategory > 0) {
            console.warn(`⚠️ Category filter "${currentCategoryFilter}" filtered out all ${beforeCategory} ingredients`);
            console.warn(`   - Available categoryIds in ingredients:`, [...new Set(ingredients.map(i => i.categoryId || i.CategoryId || '').filter(id => id))]);
        }
    } else {
        // Filter is 'all', show all ingredients (no filtering)
        console.log(`🏷️ Category filter is 'all', showing all ${ingredients.length} ingredients`);
    }
    
    // Apply expiring filter
    if (currentFilter === 'expiring') {
        const today = new Date();
        const weekFromNow = new Date();
        weekFromNow.setDate(today.getDate() + 7);
        
        const beforeExpiring = ingredients.length;
        ingredients = ingredients.filter(ing => {
            if (!ing.expiryDate) return false;
            const expiryDate = new Date(ing.expiryDate);
            return expiryDate <= weekFromNow;
        });
        console.log(`⏰ After expiring filter: ${beforeExpiring} -> ${ingredients.length}`);
    }
    
    // Apply sorting
    ingredients.sort((a, b) => {
        switch (currentSort) {
            case 'name':
                return (a.name || '').localeCompare(b.name || '');
            case 'name-desc':
                return (b.name || '').localeCompare(a.name || '');
            case 'date':
                const dateA = a.createdAt ? new Date(a.createdAt) : new Date(0);
                const dateB = b.createdAt ? new Date(b.createdAt) : new Date(0);
                return dateB - dateA;
            case 'expiry':
                if (!a.expiryDate) return 1;
                if (!b.expiryDate) return -1;
                return new Date(a.expiryDate) - new Date(b.expiryDate);
            case 'quantity':
                return (b.quantity || 0) - (a.quantity || 0);
            default:
                return 0;
        }
    });
    
    filteredIngredients = ingredients;
    console.log(`✅ Final filteredIngredients: ${filteredIngredients.length} ingredients`);
    renderIngredients();
}

function renderIngredients() {
    console.log(`🎨 renderIngredients: filteredIngredients=${filteredIngredients.length}, allIngredients=${allIngredients.length}`);
    
    const grid = document.getElementById('ingredientsGrid');
    const emptyState = document.getElementById('emptyState');
    
    if (!grid || !emptyState) {
        console.error('❌ ingredientsGrid or emptyState element not found!');
        return;
    }
    
    if (filteredIngredients.length === 0) {
        console.log('📭 No ingredients to display, showing empty state');
        console.log(`   - allIngredients: ${allIngredients.length}`);
        console.log(`   - currentCategoryFilter: ${currentCategoryFilter}`);
        console.log(`   - currentFilter: ${currentFilter}`);
        console.log(`   - currentSort: ${currentSort}`);
        if (allIngredients.length > 0) {
            console.log('   - Sample ingredient:', allIngredients[0]);
            console.log('   - First ingredient categoryId:', allIngredients[0].categoryId);
            console.log('   - First ingredient id:', allIngredients[0].id);
            console.log('   - First ingredient name:', allIngredients[0].name);
            
            // Debug: Check if filter is causing issue
            if (currentCategoryFilter !== 'all' && allIngredients.length > 0) {
                const matchingCount = allIngredients.filter(ing => {
                    const ingCategoryId = ing.categoryId || ing.CategoryId || '';
                    return ingCategoryId === currentCategoryFilter;
                }).length;
                console.warn(`⚠️ Category filter "${currentCategoryFilter}" matches ${matchingCount} out of ${allIngredients.length} ingredients`);
            }
        }
        grid.style.display = 'none';
        emptyState.style.display = 'flex';
        return;
    }
    
    console.log(`🎨 Rendering ${filteredIngredients.length} ingredients`);
    grid.style.display = 'grid';
    emptyState.style.display = 'none';
    
    const html = filteredIngredients.map((ing, index) => {
        // Ensure ID exists (should be normalized in loadIngredients)
        // If no ID, try to generate one from _id or create a temporary one
        if (!ing.id) {
            if (ing._id) {
                ing.id = ing._id;
            } else if (ing.Id) {
                ing.id = ing.Id;
            } else {
                console.warn('⚠️ Ingredient without ID (skipping):', ing);
                return ''; // Skip rendering if no ID at all
            }
        }
        
        // Normalize categoryId - should already be normalized
        const ingCategoryId = ing.categoryId || ing.CategoryId || '';
        const category = allCategories.find(c => c && c.id && (c.id === ingCategoryId || c._id === ingCategoryId));
        
        // If no category found, still render but with default values
        const categoryName = category?.name || 'Khác';
        const categoryIcon = category?.icon || '📦';
        const categoryColor = category?.color || '#667eea';
        
        const expiryWarning = getExpiryWarning(ing.expiryDate);
        
        // Escape HTML để tránh XSS
        const safeName = (ing.name || ing.Name || 'Unknown').replace(/"/g, '&quot;').replace(/'/g, '&#39;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
        const safeNotes = (ing.notes || ing.Notes || '').replace(/"/g, '&quot;').replace(/'/g, '&#39;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
        const safeId = (ing.id || '').replace(/"/g, '&quot;').replace(/'/g, '&#39;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
        
        return `
            <div class="ingredient-card" style="animation-delay: ${index * 0.05}s;">
                <div class="ingredient-header">
                    <span class="ingredient-category-badge" style="background-color: ${categoryColor}20; color: ${categoryColor};">
                        <span>${categoryIcon}</span>
                        ${categoryName}
                    </span>
                    <div class="ingredient-actions">
                        <button class="action-btn edit" onclick="editIngredient('${safeId}')" title="Sửa">
                            <i class="fas fa-edit"></i>
                        </button>
                        <button class="action-btn delete" onclick="deleteIngredient('${safeId}')" title="Xóa">
                            <i class="fas fa-trash"></i>
                        </button>
                    </div>
                </div>
                <h3 class="ingredient-name">${safeName}</h3>
                <div class="ingredient-quantity">${ing.quantity || ing.Quantity || 0} ${ing.unit || ing.Unit || 'piece'}</div>
                ${expiryWarning ? `<div class="expiry-warning ${expiryWarning.class}"><i class="fas fa-exclamation-circle"></i> ${expiryWarning.text}</div>` : ''}
                <div class="ingredient-meta">
                    ${expiryWarning ? `<div class="meta-item"><i class="fas fa-calendar"></i> HSD: ${formatDate(ing.expiryDate || ing.ExpireDate)}</div>` : ''}
                    ${safeNotes ? `<div class="meta-item"><i class="fas fa-sticky-note"></i> ${safeNotes}</div>` : ''}
                    <div class="meta-item"><i class="fas fa-clock"></i> ${formatDate(ing.createdAt || ing.CreatedAt || new Date())}</div>
                </div>
            </div>
        `;
    }).filter(html => html !== '').join('');
    
    if (!html || html.trim() === '') {
        console.error('❌ Generated HTML is empty!');
        console.error('   - filteredIngredients length:', filteredIngredients.length);
        console.error('   - First ingredient:', filteredIngredients.length > 0 ? filteredIngredients[0] : 'none');
        grid.style.display = 'none';
        emptyState.style.display = 'flex';
        return;
    }
    
    grid.innerHTML = html;
    console.log(`✅ Rendered ${filteredIngredients.length} ingredients to grid (HTML length: ${html.length} chars)`);
}

function openAddIngredientModal() {
    // Ensure categories are loaded before opening modal
    if (allCategories.length === 0) {
        showNotification('Đang tải danh mục, vui lòng đợi...', 'error');
        loadCategories().then(() => {
            if (allCategories.length > 0) {
                resetIngredientForm();
                populateCategorySelect(); // Ensure categories are populated
                showModal();
            } else {
                showNotification('Không thể tải danh mục. Vui lòng thử lại sau.', 'error');
            }
        });
        return;
    }
    
    resetIngredientForm();
    populateCategorySelect(); // Repopulate to ensure categories are current
    showModal();
    
    // Add change listener to category select for debugging
    const categorySelect = document.getElementById('ingredientCategory');
    if (categorySelect) {
        // Remove existing listener if any
        const newSelect = categorySelect.cloneNode(true);
        categorySelect.parentNode.replaceChild(newSelect, categorySelect);
        
        // Add new listener
        document.getElementById('ingredientCategory').addEventListener('change', function() {
            console.log('🔍 Category selected:', {
                value: this.value,
                text: this.options[this.selectedIndex].text,
                selectedIndex: this.selectedIndex
            });
        });
    }
}

function showModal() {
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
    
    // Validate required fields
    const name = document.getElementById('ingredientName').value.trim();
    const categorySelect = document.getElementById('ingredientCategory');
    const categoryId = categorySelect ? categorySelect.value : '';
    const quantityValue = document.getElementById('ingredientQuantity').value;
    const quantity = parseFloat(quantityValue);
    const unit = document.getElementById('ingredientUnit').value;
    
    // Debug logging - detailed
    console.log('🔍 Validation Check:', {
        name: name,
        categoryId: categoryId,
        categorySelectElement: categorySelect,
        selectedIndex: categorySelect ? categorySelect.selectedIndex : -1,
        selectedOption: categorySelect && categorySelect.options[categorySelect.selectedIndex] ? categorySelect.options[categorySelect.selectedIndex].text : 'N/A',
        allOptions: categorySelect ? Array.from(categorySelect.options).map(opt => ({ value: opt.value, text: opt.text })) : [],
        quantityValue: quantityValue,
        quantity: quantity,
        unit: unit,
        nameValid: !!name,
        categoryValid: !!categoryId && categoryId !== '' && categoryId !== null && categoryId !== undefined,
        quantityValid: !isNaN(quantity) && quantity > 0,
        unitValid: !!unit && unit !== ''
    });
    
    // Detailed validation with specific error messages
    if (!name || name.length === 0) {
        showNotification('Vui lòng nhập tên nguyên liệu', 'error');
        document.getElementById('ingredientName').focus();
        return;
    }
    
    // More thorough category validation
    if (!categoryId || categoryId === '' || categoryId === null || categoryId === undefined) {
        console.error('❌ Category validation failed:', {
            categoryId: categoryId,
            type: typeof categoryId,
            isEmpty: categoryId === '',
            isNull: categoryId === null,
            isUndefined: categoryId === undefined
        });
        showNotification('Vui lòng chọn danh mục', 'error');
        if (categorySelect) {
            categorySelect.focus();
            categorySelect.style.borderColor = '#f5576c';
        }
        return;
    }
    
    // Clear error styling if validation passes
    if (categorySelect) {
        categorySelect.style.borderColor = '';
    }
    
    // Validate quantity - check if it's a valid number > 0
    if (!quantityValue || quantityValue === '' || isNaN(quantity) || quantity <= 0) {
        showNotification('Vui lòng nhập số lượng hợp lệ (lớn hơn 0)', 'error');
        document.getElementById('ingredientQuantity').focus();
        return;
    }
    
    if (!unit || unit === '') {
        showNotification('Vui lòng chọn đơn vị', 'error');
        return;
    }
    
    // Create FormData
    const formData = new FormData();
    
    if (editingIngredientId) {
        formData.append('ingredientId', editingIngredientId);
    }
    
    formData.append('name', name);
    formData.append('categoryId', categoryId);
    formData.append('quantity', quantity.toString());
    
    // API chỉ chấp nhận unit bằng tiếng Anh: kg, g, l, ml, piece
    // Map Vietnamese units to English API enum values
    const unitMap = {
        'kg': 'kg',
        'g': 'g',
        'lít': 'l',
        'l': 'l',
        'ml': 'ml',
        'cái': 'piece',
        'củ': 'piece',
        'quả': 'piece',
        'trái': 'piece',
        'bó': 'piece',
        'gói': 'piece',
        'ổ': 'piece',
        'lon': 'piece',
        'chai': 'piece',
        'hộp': 'piece',
        'piece': 'piece'
    };
    const mappedUnit = unitMap[unit.toLowerCase()] || 'piece'; // Default to 'piece' if not found
    formData.append('unit', mappedUnit);
    console.log(`📦 Unit mapping: "${unit}" -> "${mappedUnit}"`);
    
    if (mappedUnit !== unit) {
        console.log(`⚠️ Unit was mapped from "${unit}" to "${mappedUnit}" for API compatibility`);
    }
    
    // Optional fields
    const expiryDate = document.getElementById('ingredientExpiry').value;
    if (expiryDate) {
        // Convert date format from YYYY-MM-DD to ISO string if needed
        formData.append('expireDate', expiryDate);
    }
    
    const notes = document.getElementById('ingredientNotes').value.trim();
    if (notes) {
        formData.append('notes', notes);
    }
    
    // Image file (if file input exists) or image URL
    const imageInput = document.getElementById('ingredientImageFile');
    if (imageInput && imageInput.files && imageInput.files.length > 0) {
        formData.append('image', imageInput.files[0]);
    }
    
    // Log FormData contents for debugging
    console.log('📤 Sending FormData:', {
        name: name,
        categoryId: categoryId,
        quantity: quantity.toString(),
        unit: unit,
        expireDate: expiryDate || 'none',
        notes: notes || 'none'
    });
    
    try {
        let response;
        if (editingIngredientId) {
            // PUT request - no id in URL, ingredientId is in form data
            console.log('🔄 PUT request to /api/IngredientApi');
            response = await fetch('/api/IngredientApi', {
                method: 'PUT',
                credentials: 'include',
                body: formData
            });
        } else {
            // POST request
            console.log('🔄 POST request to /api/IngredientApi');
            response = await fetch('/api/IngredientApi', {
                method: 'POST',
                credentials: 'include',
                body: formData
            });
        }
        
        console.log('📥 Response status:', response.status, response.statusText);
        
        if (!response.ok) {
            const errorText = await response.text();
            console.error('❌ Error response:', errorText);
            let errorMessage = 'Lỗi khi lưu nguyên liệu';
            try {
                const errorJson = JSON.parse(errorText);
                errorMessage = errorJson.message || errorJson.error || errorMessage;
            } catch {
                errorMessage = errorText || errorMessage;
            }
            throw new Error(errorMessage);
        }
        
        const result = await response.json();
        console.log('✅ Success response:', result);
        showNotification(editingIngredientId ? 'Đã cập nhật nguyên liệu' : 'Đã thêm nguyên liệu mới', 'success');
        await loadIngredients();
        closeIngredientModal();
    } catch (error) {
        console.error('❌ Error saving ingredient:', error);
        showNotification('Lỗi: ' + error.message, 'error');
    }
}

async function editIngredient(id) {
    const ingredient = allIngredients.find(i => i.id === id);
    if (!ingredient) return;
    
    // Normalize expiry date field (handle both expiryDate and ExpireDate)
    const expiryDate = ingredient.expiryDate || ingredient.ExpireDate;
    
    editingIngredientId = id;
    document.getElementById('ingredientId').value = id;
    document.getElementById('ingredientName').value = ingredient.name;
    document.getElementById('ingredientCategory').value = ingredient.categoryId;
    document.getElementById('ingredientQuantity').value = ingredient.quantity;
    
    // Map API unit (English) back to display value for editing
    // API returns: kg, g, l, ml, piece
    // Form displays: kg, g, l, ml, piece (same now)
    const apiUnit = ingredient.unit || 'piece';
    document.getElementById('ingredientUnit').value = apiUnit;
    document.getElementById('ingredientExpiry').value = expiryDate ? expiryDate.split('T')[0] : '';
    document.getElementById('ingredientNotes').value = ingredient.notes || '';
    document.getElementById('ingredientImage').value = ingredient.imageUrl || '';
    
    document.getElementById('ingredientModalTitle').textContent = 'Sửa Nguyên Liệu';
    document.getElementById('ingredientSubmitText').textContent = 'Cập Nhật';
    
    document.getElementById('ingredientModal').classList.add('active');
}

async function deleteIngredient(id) {
    if (!id) {
        console.error('❌ Delete ingredient called with empty ID');
        showNotification('Lỗi: Không có ID nguyên liệu', 'error');
        return;
    }
    
    const ingredient = allIngredients.find(i => i.id === id);
    if (!ingredient) {
        console.error('❌ Ingredient not found:', id);
        showNotification('Không tìm thấy nguyên liệu', 'error');
        return;
    }
    
    if (!confirm(`Bạn có chắc muốn xóa "${ingredient.name}"?`)) return;
    
    try {
        console.log('🗑️ Deleting ingredient:', id);
        const response = await fetch(`/api/IngredientApi/${id}`, {
            method: 'DELETE',
            credentials: 'include',
            headers: {
                'Content-Type': 'application/json'
            }
        });
        
        const responseText = await response.text();
        console.log('📥 Delete response:', response.status, responseText);
        
        if (!response.ok) {
            let errorMessage = 'Không thể xóa nguyên liệu';
            try {
                const errorJson = JSON.parse(responseText);
                errorMessage = errorJson.message || errorJson.error || errorMessage;
            } catch (e) {
                errorMessage = responseText || errorMessage;
            }
            throw new Error(errorMessage);
        }
        
        showNotification('Đã xóa nguyên liệu thành công', 'success');
        await loadIngredients();
    } catch (error) {
        console.error('❌ Error deleting ingredient:', error);
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

