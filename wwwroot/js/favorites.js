class FavoriteManager {
    constructor(notificationCallback) {
        this.notification = notificationCallback;
        this.favoritesCache = new Set(); // Store recipeIds of favorited items
        this.loadFavoritesCache();
    }

    async loadFavoritesCache() {
        try {
            const response = await fetch('/api/FavoriteApi', { credentials: 'include' });
            if (response.ok) {
                const result = await response.json();
                // CookMate API returns array directly
                this.favoritesCache = new Set(result.map(fav => fav.recipeId));
                this.updateAllFavoriteButtons();
                console.log('✅ Loaded favorites from CookMate API:', this.favoritesCache.size);
            }
        } catch (error) {
            console.error('Error loading favorites cache:', error);
        }
    }

    async isFavorite(recipeId) {
        return this.favoritesCache.has(recipeId);
    }

    async toggleFavorite(recipeData, buttonElement) {
        const recipeId = recipeData.id;
        if (await this.isFavorite(recipeId)) {
            // Remove from favorites
            try {
                const response = await fetch(`/api/FavoriteApi/recipe/${recipeId}`, {
                    method: 'DELETE',
                    headers: { 'Content-Type': 'application/json' },
                    credentials: 'include'
                });

                if (!response.ok) {
                    throw new Error('Failed to remove from favorites');
                }

                this.favoritesCache.delete(recipeId);
                this.updateFavoriteButton(buttonElement, false);
                this.notification(`Đã xóa "${recipeData.name}" khỏi yêu thích!`, 'info');
                console.log('✅ Removed from favorites via CookMate API');
                return false; // Not a favorite anymore
            } catch (error) {
                console.error('Error removing favorite:', error);
                this.notification('Có lỗi xảy ra khi xóa khỏi yêu thích!', 'error');
                return true; // Still a favorite (failed to remove)
            }
        } else {
            // Add to favorites
            try {
                const favorite = {
                    recipeId: recipeData.id,
                    recipeName: recipeData.name,
                    recipeDescription: recipeData.description,
                    recipeCategory: recipeData.category,
                    recipeImage: recipeData.image,
                    cookingTime: recipeData.time,
                    difficulty: recipeData.difficulty,
                    rating: recipeData.rating
                };

                // CookMate API only needs recipeId
                console.log('🔄 Adding favorite:', recipeData.id);
                const response = await fetch('/api/FavoriteApi', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ recipeId: recipeData.id }),
                    credentials: 'include'
                });

                console.log('📡 Add favorite response status:', response.status, response.statusText);

                if (!response.ok) {
                    const errorText = await response.text();
                    console.error('❌ Failed to add favorite:', response.status, errorText);
                    let errorData;
                    try {
                        errorData = JSON.parse(errorText);
                    } catch {
                        errorData = { message: errorText };
                    }
                    throw new Error(errorData.message || errorData.error || 'Failed to add to favorites');
                }

                this.favoritesCache.add(recipeId);
                this.updateFavoriteButton(buttonElement, true);
                this.notification(`Đã thêm "${recipeData.name}" vào yêu thích!`, 'success');
                console.log('✅ Added to favorites via CookMate API');
                return true; // Now a favorite
            } catch (error) {
                console.error('Error adding favorite:', error);
                this.notification(`Có lỗi xảy ra khi thêm vào yêu thích: ${error.message}`, 'error');
                return false; // Not a favorite (failed to add)
            }
        }
    }

    updateFavoriteButton(buttonElement, isFavorite) {
        if (buttonElement) {
            if (isFavorite) {
                buttonElement.classList.add('favorited');
                buttonElement.innerHTML = `<svg width="20" height="20" viewBox="0 0 24 24" fill="currentColor" stroke="currentColor" stroke-width="2"><path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"></path></svg>`;
            } else {
                buttonElement.classList.remove('favorited');
                buttonElement.innerHTML = `<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z"></path></svg>`;
            }
        }
    }

    updateAllFavoriteButtons() {
        document.querySelectorAll('.favorite-toggle-btn').forEach(button => {
            const recipeId = button.closest('.recipe-card').getAttribute('data-id');
            this.updateFavoriteButton(button, this.favoritesCache.has(recipeId));
        });
    }
}

