// Lặp qua tất cả các phần tử có lớp .selected và thêm sự kiện click
document.querySelectorAll('.selected').forEach(selected => {
    selected.addEventListener('click', function() {
        // Đóng tất cả các dropdown khác trước khi mở dropdown hiện tại
        document.querySelectorAll('.options').forEach(option => option.classList.remove('show'));
        this.nextElementSibling.classList.toggle('show');
    });
});

// Xử lý sự kiện click cho các tùy chọn trong mỗi dropdown
document.querySelectorAll('.options li').forEach(item => {
    item.addEventListener('click', function() {
        const selectedValue = this.getAttribute('data-value');
        const customSelect = this.closest('.custom-select');
        customSelect.querySelector('.selected').textContent = this.textContent;
        customSelect.querySelector('.options').classList.remove('show');
        console.log('Selected value:', selectedValue);
    });
});

// Đóng dropdown khi click bên ngoài
window.addEventListener('click', function(e) {
    document.querySelectorAll('.custom-select').forEach(select => {
        if (!select.contains(e.target)) {
            select.querySelector('.options').classList.remove('show');
        }
    });
});


// Xử lý sự kiện click cho các tùy chọn trong mỗi dropdown
document.querySelectorAll('.options li').forEach(item => {
    item.addEventListener('click', function() {
        const selectedValue = this.getAttribute('data-value');
        const customSelect = this.closest('.custom-select');
        
        // Cập nhật giá trị của phần tử .selected
        customSelect.querySelector('.selected').textContent = this.textContent;

        // Cập nhật giá trị vào input ẩn tương ứng
        if (customSelect.closest('.col-md-3').querySelector('label').textContent.includes("Điểm đi")) {
            customSelect.querySelector('#departure').value = selectedValue; // Lưu vào input ẩn "Điểm đi"
        } else {
            customSelect.querySelector('#destination').value = selectedValue; // Lưu vào input ẩn "Điểm đến"
        }

        // Đóng dropdown
        customSelect.querySelector('.options').classList.remove('show');
        console.log('Selected value:', selectedValue); // In ra giá trị đã chọn
    });
});


const loginModal = document.querySelector('.login-form');
const overlay = document.querySelector('.overlay');
const registerLink = document.querySelector('.register-link');
const registerForm = document.querySelector('.register-form');



const userButton = document.querySelector('.user-icon');








function showLoginForm() {
    loginModal.classList.add('show');
    overlay.classList.add('show');
    document.body.classList.add('modal-open');
}


function closeLoginForm() {
    loginModal.classList.remove('show');
    overlay.classList.remove('show');
    document.body.classList.remove('modal-open');
}

// mở form đk
function showRegisterForm() {
    registerForm.classList.add('show');
    loginModal.classList.remove('show');
}

// đóng form đk 
function closeRegisterForm() {
    registerForm.classList.remove('show');
    overlay.classList.remove('show');
    document.body.classList.remove('modal-open');
}



if (registerLink) {
    registerLink.addEventListener('click', function (event) {
        event.preventDefault(); 
        showRegisterForm();
    });
}



if (overlay) {
    overlay.addEventListener('click', function () {
        closeLoginForm();
        closeRegisterForm();
        
    });
    
}



