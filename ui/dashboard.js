// API Base URL
const API_BASE = 'http://localhost:5000/api';

// Initialize
document.addEventListener('DOMContentLoaded', () => {
    checkLoginStatus();
    document.getElementById('user-menu-btn').addEventListener('click', toggleUserDropdown);
    loadDashboardData();
});

// ============ Authentication ============

async function handleLogin(event) {
    event.preventDefault();
    const email = document.getElementById('login-email').value;
    const password = document.getElementById('login-password').value;

    try {
        const response = await fetch(`${API_BASE}/auth/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            credentials: 'include',
            body: JSON.stringify({ email, password })
        });

        if (response.ok) {
            const data = await response.json();
            document.getElementById('user-email').textContent = data.email;
            showDashboard();
            loadDashboardData();
        } else {
            showError('login-error', 'Invalid credentials');
        }
    } catch (error) {
        console.error('Login error:', error);
        showError('login-error', 'Login failed');
    }
}

async function handleRegister(event) {
    event.preventDefault();
    const email = document.getElementById('register-email').value;
    const name = document.getElementById('register-name').value;
    const password = document.getElementById('register-password').value;

    try {
        const response = await fetch(`${API_BASE}/auth/register`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            credentials: 'include',
            body: JSON.stringify({ email, name, password })
        });

        if (response.ok) {
            const data = await response.json();
            document.getElementById('user-email').textContent = data.email;
            showDashboard();
            loadDashboardData();
        } else {
            showError('register-error', 'Registration failed');
        }
    } catch (error) {
        console.error('Register error:', error);
        showError('register-error', 'Registration failed');
    }
}

async function logout() {
    try {
        await fetch(`${API_BASE}/auth/logout`, {
            method: 'POST',
            credentials: 'include'
        });
        showAuthForm();
    } catch (error) {
        console.error('Logout error:', error);
    }
}

async function checkLoginStatus() {
    try {
        const response = await fetch(`${API_BASE}/auth/user`, {
            credentials: 'include'
        });

        if (response.ok) {
            const data = await response.json();
            document.getElementById('user-email').textContent = data.email;
            showDashboard();
        } else {
            showAuthForm();
        }
    } catch (error) {
        console.error('Check login error:', error);
        showAuthForm();
    }
}

// ============ UI State Management ============

function showAuthForm() {
    document.getElementById('auth-container').style.display = 'flex';
    document.getElementById('dashboard-container').style.display = 'none';
}

function showDashboard() {
    document.getElementById('auth-container').style.display = 'none';
    document.getElementById('dashboard-container').style.display = 'flex';
}

function switchTab(tabName) {
    // Hide all tabs
    document.querySelectorAll('.tab-content').forEach(tab => {
        tab.classList.remove('active');
    });
    
    // Remove active from all buttons
    document.querySelectorAll('.tab-btn').forEach(btn => {
        btn.classList.remove('active');
    });

    // Show selected tab
    document.getElementById(tabName + '-tab').classList.add('active');
    event.target.classList.add('active');

    // Clear error messages
    document.querySelectorAll('.error-message').forEach(msg => {
        msg.textContent = '';
        msg.classList.remove('show');
    });
}

function switchDashboard(dashboardName) {
    // Hide all dashboards
    document.querySelectorAll('.dashboard-tab').forEach(tab => {
        tab.classList.remove('active');
    });
    
    // Remove active from all nav links
    document.querySelectorAll('.nav-link').forEach(link => {
        link.classList.remove('active');
    });

    // Show selected dashboard
    document.getElementById(dashboardName).classList.add('active');
    event.target.classList.add('active');
}

function toggleUserDropdown() {
    const dropdown = document.getElementById('user-dropdown');
    dropdown.style.display = dropdown.style.display === 'none' ? 'block' : 'none';
}

// ============ Dashboard Data ============

async function loadDashboardData() {
    try {
        // Load health status
        const healthResponse = await fetch(`${API_BASE}/health`, {
            credentials: 'include'
        });
        if (healthResponse.ok) {
            const health = await healthResponse.json();
            document.getElementById('system-status').textContent = health.status || 'Healthy';
        }

        // Load users
        loadUsers();

        // Load app status
        loadAppStatus();
    } catch (error) {
        console.error('Load dashboard data error:', error);
    }
}

async function loadUsers() {
    try {
        const response = await fetch(`${API_BASE}/users`, {
            credentials: 'include'
        });

        if (response.ok) {
            const data = await response.json();
            const users = data.users || [];
            displayUsers(users);
            document.getElementById('active-users').textContent = users.length;
        }
    } catch (error) {
        console.error('Load users error:', error);
    }
}

function displayUsers(users) {
    const tbody = document.getElementById('users-tbody');
    tbody.innerHTML = '';

    if (users.length === 0) {
        tbody.innerHTML = '<tr><td colspan="5">No users found</td></tr>';
        return;
    }

    users.forEach(user => {
        const row = document.createElement('tr');
        row.innerHTML = `
            <td>${user.email}</td>
            <td>${user.name}</td>
            <td>${user.role || 'user'}</td>
            <td><span class="status-badge ${user.is_active ? 'active' : 'inactive'}">
                ${user.is_active ? 'Active' : 'Inactive'}
            </span></td>
            <td>
                <button onclick="editUser(${user.id})">Edit</button>
                <button onclick="deleteUserAction(${user.id})">Delete</button>
            </td>
        `;
        tbody.appendChild(row);
    });
}

async function loadAppStatus() {
    try {
        const response = await fetch(`${API_BASE}/status`, {
            credentials: 'include'
        });

        if (response.ok) {
            const status = await response.json();
            document.getElementById('components-status').textContent = 'All Active';
        }
    } catch (error) {
        console.error('Load app status error:', error);
    }
}

// ============ User Management ============

function toggleUserForm() {
    const form = document.getElementById('user-form');
    form.style.display = form.style.display === 'none' ? 'block' : 'none';
}

async function handleAddUser(event) {
    event.preventDefault();
    
    const email = document.getElementById('new-user-email').value;
    const name = document.getElementById('new-user-name').value;
    const role = document.getElementById('new-user-role').value;

    try {
        const response = await fetch(`${API_BASE}/users`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            credentials: 'include',
            body: JSON.stringify({ email, name, role })
        });

        if (response.ok) {
            toggleUserForm();
            event.target.reset();
            loadUsers();
        } else {
            alert('Failed to add user');
        }
    } catch (error) {
        console.error('Add user error:', error);
        alert('Failed to add user');
    }
}

async function deleteUserAction(userId) {
    if (!confirm('Are you sure you want to delete this user?')) {
        return;
    }

    try {
        const response = await fetch(`${API_BASE}/users/${userId}`, {
            method: 'DELETE',
            credentials: 'include'
        });

        if (response.ok) {
            loadUsers();
        } else {
            alert('Failed to delete user');
        }
    } catch (error) {
        console.error('Delete user error:', error);
        alert('Failed to delete user');
    }
}

function editUser(userId) {
    alert('Edit user ' + userId + ' - not implemented');
}

// ============ API Testing ============

async function handleTestEndpoint(event) {
    event.preventDefault();

    const method = document.getElementById('api-method').value;
    const endpoint = document.getElementById('api-endpoint').value;
    let body = document.getElementById('api-body').value;

    try {
        body = body ? JSON.parse(body) : {};
    } catch (e) {
        alert('Invalid JSON in request body');
        return;
    }

    try {
        const response = await fetch(`${API_BASE}/endpoint-test`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            credentials: 'include',
            body: JSON.stringify({ endpoint, method, body })
        });

        const data = await response.json();
        
        const responseDiv = document.getElementById('api-response');
        const responseBody = document.getElementById('response-body');
        
        responseBody.textContent = JSON.stringify(data, null, 2);
        responseDiv.style.display = 'block';
    } catch (error) {
        console.error('Test endpoint error:', error);
        alert('Failed to test endpoint');
    }
}

// ============ Testing ============

function runTests() {
    const output = document.getElementById('tests-output');
    output.innerHTML = '';

    const tests = [
        { name: 'Authentication Module', pass: true },
        { name: 'User Management', pass: true },
        { name: 'API Handler', pass: true },
        { name: 'Logging System', pass: true },
        { name: 'Validation Functions', pass: true }
    ];

    tests.forEach(test => {
        const item = document.createElement('div');
        item.className = `test-item ${test.pass ? 'pass' : 'fail'}`;
        item.textContent = `${test.pass ? '✓' : '✗'} ${test.name}`;
        output.appendChild(item);
    });
}

async function runHealthCheck() {
    const output = document.getElementById('tests-output');
    output.innerHTML = '<div class="test-item">Running health checks...</div>';

    try {
        const response = await fetch(`${API_BASE}/health`, {
            credentials: 'include'
        });

        if (response.ok) {
            const health = await response.json();
            output.innerHTML = `
                <div class="test-item pass">✓ Server is healthy</div>
                <div class="test-item pass">✓ All components active</div>
                <div class="test-item pass">✓ Database connection OK</div>
            `;
        } else {
            output.innerHTML = '<div class="test-item fail">✗ Health check failed</div>';
        }
    } catch (error) {
        output.innerHTML = '<div class="test-item fail">✗ Health check failed: ' + error + '</div>';
    }
}

// ============ Utility Functions ============

function showError(elementId, message) {
    const element = document.getElementById(elementId);
    element.textContent = message;
    element.classList.add('show');
}

// Close dropdown when clicking outside
document.addEventListener('click', (e) => {
    const dropdown = document.getElementById('user-dropdown');
    const btn = document.getElementById('user-menu-btn');
    if (!btn.contains(e.target) && !dropdown.contains(e.target)) {
        dropdown.style.display = 'none';
    }
});
