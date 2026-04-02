// Repo1 Dashboard - Main JavaScript
// Connects with Flask backend API

const API_BASE = 'http://localhost:5000/api';

// Initialize dashboard on page load
document.addEventListener('DOMContentLoaded', initDashboard);

async function initDashboard() {
    console.log('📊 Initializing Repo1 Dashboard...');
    
    try {
        // Check server health
        const healthCheck = await fetch(`${API_BASE}/health`);
        if (!healthCheck.ok) throw new Error('Server not accessible');
        
        console.log('✅ Server connected successfully');
        
        // Load statistics
        await loadStats();
        
        // Load folder details
        await loadFolders();
        
        // Setup event listeners
        setupEventListeners();
        
        console.log('✅ Dashboard initialized successfully');
    } catch (error) {
        console.error('❌ Dashboard initialization error:', error);
        showErrorMessage('Failed to initialize dashboard. Is the Flask server running?');
    }
}

// Load repository statistics
async function loadStats() {
    try {
        const response = await fetch(`${API_BASE}/stats`);
        if (!response.ok) throw new Error('Failed to fetch stats');
        
        const data = await response.json();
        
        // Update statistics cards
        const totalFilesCard = document.getElementById('total-files');
        if (totalFilesCard) {
            totalFilesCard.textContent = data.total_files.toLocaleString();
        }
        
        console.log(`✅ Loaded stats: ${data.total_files} total files`);
    } catch (error) {
        console.error('❌ Error loading stats:', error);
    }
}

// Load folder information
async function loadFolders() {
    try {
        const response = await fetch(`${API_BASE}/folders`);
        if (!response.ok) throw new Error('Failed to fetch folders');
        
        const data = await response.json();
        
        // Update folder cards
        const foldersContainer = document.getElementById('folders-grid');
        if (foldersContainer) {
            foldersContainer.innerHTML = '';
            
            data.folders.forEach(folder => {
                const card = createFolderCard(folder);
                foldersContainer.appendChild(card);
            });
        }
        
        console.log(`✅ Loaded ${data.folders.length} folders`);
    } catch (error) {
        console.error('❌ Error loading folders:', error);
    }
}

// Create a folder card element
function createFolderCard(folder) {
    const card = document.createElement('div');
    card.className = 'folder-card';
    card.style.cursor = 'pointer';
    
    const icons = {
        'auth': '🔐',
        'ui': '🎨',
        'api': '🔗',
        'utils': '🛠️',
        'tests': '✔️'
    };
    
    const fileTypesHtml = Object.entries(folder.file_types)
        .slice(0, 5)
        .map(([ext, count]) => `<span style="font-size: 12px; color: #9ca3af; margin-right: 8px;">• ${ext}: ${count}</span>`)
        .join('');
    
    card.innerHTML = `
        <div class="folder-icon">${icons[folder.name] || '📁'}</div>
        <div class="folder-name">${folder.name}/</div>
        <div class="folder-desc">${folder.description}</div>
        <div class="folder-count">${folder.file_count.toLocaleString()} files</div>
        <div style="margin-top: 12px; padding-top: 12px; border-top: 1px solid #e5e7eb; font-size: 12px;">
            ${fileTypesHtml}
        </div>
    `;
    
    card.addEventListener('click', () => showFolderDetails(folder.name));
    
    return card;
}

// Show folder details
async function showFolderDetails(folderName) {
    try {
        const response = await fetch(`${API_BASE}/folder/${folderName}`);
        if (!response.ok) throw new Error('Failed to fetch folder details');
        
        const data = await response.json();
        
        // Create modal or sidebar with details
        showModal({
            title: `${folderName}/ - Folder Details`,
            content: createFolderDetailsContent(data)
        });
        
        console.log(`✅ Loaded details for ${folderName}`);
    } catch (error) {
        console.error('❌ Error loading folder details:', error);
        showErrorMessage(`Failed to load details for ${folderName}`);
    }
}

// Create folder details content
function createFolderDetailsContent(folder) {
    let html = `
        <div style="padding: 20px;">
            <h3 style="margin-bottom: 12px; color: #1f2937;">${folder.folder}/</h3>
            <p style="color: #6b7280; margin-bottom: 16px;">${folder.description}</p>
            
            <div style="background: #f3f4f6; padding: 16px; border-radius: 8px; margin-bottom: 16px;">
                <div style="font-size: 14px; color: #6b7280;">File Count</div>
                <div style="font-size: 32px; font-weight: bold; color: #667eea;">${folder.file_count.toLocaleString()}</div>
            </div>
            
            <div style="margin-bottom: 16px;">
                <h4 style="margin-bottom: 12px; color: #1f2937; font-size: 16px;">File Types Distribution</h4>
                <div style="display: grid; grid-template-columns: repeat(auto-fill, minmax(100px, 1fr)); gap: 12px;">
    `;
    
    Object.entries(folder.file_types)
        .sort((a, b) => b[1] - a[1])
        .slice(0, 10)
        .forEach(([ext, count]) => {
            html += `
                <div style="background: white; padding: 12px; border-radius: 6px; border-left: 3px solid #667eea; text-align: center;">
                    <div style="font-size: 12px; color: #6b7280;">${ext}</div>
                    <div style="font-size: 18px; font-weight: bold; color: #667eea;">${count}</div>
                </div>
            `;
        });
    
    html += `
                </div>
            </div>
    `;
    
    if (folder.sample_files && folder.sample_files.length > 0) {
        html += `
            <div style="margin-bottom: 16px;">
                <h4 style="margin-bottom: 12px; color: #1f2937; font-size: 16px;">Sample Files</h4>
                <div style="background: #f9fafb; padding: 12px; border-radius: 6px; max-height: 200px; overflow-y: auto;">
        `;
        
        folder.sample_files.forEach(file => {
            const fileName = file.split('\\').pop();
            html += `<div style="padding: 6px 0; color: #4b5563; font-size: 12px;">📄 ${fileName}</div>`;
        });
        
        html += `
                </div>
            </div>
        `;
    }
    
    html += `</div>`;
    
    return html;
}

// Show modal
function showModal(options) {
    let modal = document.getElementById('modal-overlay');
    
    if (!modal) {
        modal = document.createElement('div');
        modal.id = 'modal-overlay';
        modal.style.cssText = `
            display: none;
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: rgba(0, 0, 0, 0.5);
            z-index: 1000;
            align-items: center;
            justify-content: center;
        `;
        document.body.appendChild(modal);
    }
    
    let modalContent = document.getElementById('modal-content');
    
    if (!modalContent) {
        modalContent = document.createElement('div');
        modalContent.id = 'modal-content';
        modal.appendChild(modalContent);
    }
    
    modalContent.style.cssText = `
        background: white;
        border-radius: 12px;
        width: 90%;
        max-width: 600px;
        max-height: 80vh;
        overflow-y: auto;
        box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
    `;
    
    modalContent.innerHTML = `
        <div style="padding: 20px; border-bottom: 1px solid #e5e7eb; display: flex; justify-content: space-between; align-items: center; background: #f9fafb;">
            <h2 style="margin: 0; color: #1f2937;">${options.title}</h2>
            <button onclick="closeModal()" style="background: none; border: none; font-size: 24px; cursor: pointer; color: #6b7280;">✕</button>
        </div>
        ${options.content}
    `;
    
    modal.style.display = 'flex';
}

// Close modal
function closeModal() {
    const modal = document.getElementById('modal-overlay');
    if (modal) {
        modal.style.display = 'none';
    }
}

// Setup event listeners
function setupEventListeners() {
    // Search functionality
    const searchInput = document.getElementById('search-input');
    if (searchInput) {
        searchInput.addEventListener('keyup', debounce((e) => {
            if (e.key === 'Enter') {
                performSearch(e.target.value);
            }
        }, 300));
    }
    
    // Close modal on background click
    const modal = document.getElementById('modal-overlay');
    if (modal) {
        modal.addEventListener('click', (e) => {
            if (e.target === modal) {
                closeModal();
            }
        });
    }
}

// Perform file search
async function performSearch(filename) {
    if (!filename.trim()) return;
    
    try {
        const response = await fetch(`${API_BASE}/search/${filename}`);
        if (!response.ok) throw new Error('Search failed');
        
        const data = await response.json();
        
        showModal({
            title: `Search Results for "${filename}" (${data.total_found} found)`,
            content: createSearchResultsContent(data.results)
        });
    } catch (error) {
        console.error('❌ Search error:', error);
        showErrorMessage('Search failed');
    }
}

// Create search results content
function createSearchResultsContent(results) {
    if (results.length === 0) {
        return `
            <div style="padding: 40px; text-align: center; color: #6b7280;">
                <p>No files found matching your search.</p>
            </div>
        `;
    }
    
    let html = `<div style="padding: 20px;"><div style="display: grid; grid-template-columns: 1fr; gap: 12px;">`;
    
    results.forEach(file => {
        const size = formatFileSize(file.size);
        html += `
            <div style="background: #f9fafb; padding: 12px; border-radius: 6px; border-left: 3px solid #667eea;">
                <div style="font-weight: 600; color: #1f2937; margin-bottom: 4px;">📄 ${file.name}</div>
                <div style="font-size: 12px; color: #6b7280;">${file.path}</div>
                <div style="font-size: 12px; color: #9ca3af; margin-top: 4px;">Size: ${size}</div>
            </div>
        `;
    });
    
    html += `</div></div>`;
    
    return html;
}

// Show error message
function showErrorMessage(message) {
    const notification = document.createElement('div');
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        background: #ef4444;
        color: white;
        padding: 16px 24px;
        border-radius: 8px;
        max-width: 400px;
        z-index: 2000;
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.2);
    `;
    notification.textContent = message;
    document.body.appendChild(notification);
    
    setTimeout(() => {
        notification.remove();
    }, 5000);
}

// Format file size
function formatFileSize(bytes) {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round((bytes / Math.pow(k, i)) * 100) / 100 + ' ' + sizes[i];
}

// Debounce function
function debounce(func, wait) {
    let timeout;
    return function(...args) {
        clearTimeout(timeout);
        timeout = setTimeout(() => func.apply(this, args), wait);
    };
}

// Log initialization complete
console.log('✅ Repo1 Dashboard JavaScript loaded');
console.log(`📍 API Base: ${API_BASE}`);

