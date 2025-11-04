// Chat functionality
document.addEventListener('DOMContentLoaded', function() {
    const messageInput = document.getElementById('messageInput');
    const sendBtn = document.getElementById('sendBtn');
    const chatMessages = document.getElementById('chatMessages');
    const newChatBtn = document.getElementById('newChatBtn');
    const searchChatBtn = document.getElementById('searchChatBtn');
    const historyChatBtn = document.getElementById('historyChatBtn');
    const chatHistory = document.getElementById('chatHistory');
    
    let currentChatId = Date.now();
    let messages = [];

    // Send message function
    function sendMessage() {
        const message = messageInput.value.trim();
        if (!message) return;

        // Add user message
        addMessage(message, 'user');
        messageInput.value = '';

        // Simulate AI response
        setTimeout(() => {
            const response = generateResponse(message);
            addMessage(response, 'assistant');
        }, 1000);
    }

    // Add message to chat
    function addMessage(text, sender) {
        const messageDiv = document.createElement('div');
        messageDiv.className = `message ${sender}`;
        
        const contentDiv = document.createElement('div');
        contentDiv.className = 'message-content';
        contentDiv.textContent = text;
        
        messageDiv.appendChild(contentDiv);
        chatMessages.appendChild(messageDiv);
        
        // Scroll to bottom
        chatMessages.scrollTop = chatMessages.scrollHeight;
        
        // Store message
        messages.push({ text, sender, timestamp: new Date() });
    }

    // Generate AI response
    function generateResponse(userMessage) {
        const responses = {
            'hello': 'Xin chào! Tôi là CookMate, trợ lý nấu ăn của bạn. Bạn muốn hỏi gì về nấu ăn?',
            'phở': 'Phở là món ăn truyền thống của Việt Nam! Bạn muốn học cách nấu phở bò hay phở gà?',
            'bánh mì': 'Bánh mì Việt Nam rất ngon! Tôi có thể hướng dẫn bạn làm bánh mì thịt nướng hoặc bánh mì pate.',
            'cách nấu': 'Tôi sẽ hướng dẫn bạn từng bước! Bạn muốn nấu món gì cụ thể?',
            'công thức': 'Tôi có rất nhiều công thức nấu ăn! Bạn quan tâm đến món nào?',
            'món việt': 'Món ăn Việt Nam rất đa dạng! Bạn thích món nào: phở, bún bò, cơm tấm, hay bánh xèo?',
            'default': 'Cảm ơn bạn đã hỏi! Tôi có thể giúp bạn với các công thức nấu ăn, mẹo vặt trong bếp, hoặc tư vấn về dinh dưỡng. Bạn muốn biết gì cụ thể?'
        };

        const lowerMessage = userMessage.toLowerCase();
        
        for (const [key, response] of Object.entries(responses)) {
            if (lowerMessage.includes(key)) {
                return response;
            }
        }
        
        return responses.default;
    }

    // New chat function
    function startNewChat() {
        // Clear current chat
        chatMessages.innerHTML = `
            <div class="welcome-message">
                <h2>Chào mừng đến với CookMate!</h2>
                <p>Hãy hỏi tôi bất cứ điều gì về nấu ăn, công thức, hoặc món ăn bạn muốn.</p>
            </div>
        `;
        
        // Reset messages
        messages = [];
        currentChatId = Date.now();
        
        // Update active state
        document.querySelectorAll('.nav-item').forEach(item => item.classList.remove('active'));
        newChatBtn.classList.add('active');
        
        // Hide history
        chatHistory.style.display = 'none';
    }

    // Search chat function
    function searchChat() {
        const searchTerm = prompt('Nhập từ khóa để tìm kiếm trong lịch sử chat:');
        if (searchTerm) {
            // Simulate search
            alert(`Đang tìm kiếm: "${searchTerm}" trong lịch sử chat...`);
        }
    }

    // Show history function
    function showHistory() {
        chatHistory.style.display = chatHistory.style.display === 'none' ? 'block' : 'none';
        
        // Update active state
        document.querySelectorAll('.nav-item').forEach(item => item.classList.remove('active'));
        historyChatBtn.classList.add('active');
    }

    // Event listeners
    sendBtn.addEventListener('click', sendMessage);
    
    messageInput.addEventListener('keypress', function(e) {
        if (e.key === 'Enter') {
            sendMessage();
        }
    });

    newChatBtn.addEventListener('click', startNewChat);
    searchChatBtn.addEventListener('click', searchChat);
    historyChatBtn.addEventListener('click', showHistory);

    // History item clicks
    document.querySelectorAll('.history-item').forEach(item => {
        item.addEventListener('click', function() {
            const topic = this.textContent;
            messageInput.value = topic;
            messageInput.focus();
        });
    });

    // Auto-focus input
    messageInput.focus();
});
