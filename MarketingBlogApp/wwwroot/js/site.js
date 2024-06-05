document.addEventListener('DOMContentLoaded', () => {
    function clearErrors() {
        const errorElements = document.querySelectorAll('.error-message');
        errorElements.forEach(element => {
            element.innerText = '';
            element.style.display = 'none'; // Hide the error message
        });
    }

    function validateForm(formType) {
        clearErrors();
        let messages = [];
        let isValid = true; // Flag to check form validity

        if (formType === 'register') {
            const FirstName = document.getElementById('FirstName');
            const LastName = document.getElementById('LastName');
            const UserName = document.getElementById('UserName');
            const Email = document.getElementById('Email');
            const Address = document.getElementById('Address');
            const Password = document.getElementById('PasswordRegister');
            const ConfirmPassword = document.getElementById('ConfirmPassword');

            if (FirstName.value.trim() === '') {
                const errorElement = document.getElementById('FirstNameError');
                errorElement.innerText = 'First Name is required';
                errorElement.style.display = 'block'; // Show the error message
                isValid = false;
            }
            if (LastName.value.trim() === '') {
                const errorElement = document.getElementById('LastNameError');
                errorElement.innerText = 'Last Name is required';
                errorElement.style.display = 'block'; // Show the error message
                isValid = false;
            }
            if (UserName.value.trim() === '') {
                const errorElement = document.getElementById('UserNameError');
                errorElement.innerText = 'User Name is required';
                errorElement.style.display = 'block'; // Show the error message
                isValid = false;
            }
            if (Email.value.trim() === '') {
                const errorElement = document.getElementById('EmailError');
                errorElement.innerText = 'Email is required';
                errorElement.style.display = 'block'; // Show the error message
                isValid = false;
            }
            if (Address.value.trim() === '') {
                const errorElement = document.getElementById('AddressError');
                errorElement.innerText = 'Address is required';
                errorElement.style.display = 'block'; // Show the error message
                isValid = false;
            }
            if (Password.value.length <= 6) {
                const errorElement = document.getElementById('PasswordRegisterError');
                errorElement.innerText = 'Password must be longer than six characters';
                errorElement.style.display = 'block'; // Show the error message
                isValid = false;
            }
            if (Password.value.length >= 10) {
                const errorElement = document.getElementById('PasswordRegisterError');
                errorElement.innerText = 'Password must be shorter than ten characters';
                errorElement.style.display = 'block'; // Show the error message
                isValid = false;
            }
            if (Password.value.toLowerCase() === 'password') {
                const errorElement = document.getElementById('PasswordRegisterError');
                errorElement.innerText = 'Password cannot be "password"';
                errorElement.style.display = 'block'; // Show the error message
                isValid = false;
            }
            if (Password.value !== ConfirmPassword.value) {
                const errorElement = document.getElementById('ConfirmPasswordError');
                errorElement.innerText = 'Passwords do not match';
                errorElement.style.display = 'block'; // Show the error message
                isValid = false;
            }
        } else if (formType === 'login') {
            const Identifier = document.getElementById('Identifier');
            const Password = document.getElementById('PasswordLogin');

            if (Identifier.value.trim() === '') {
                const errorElement = document.getElementById('IdentifierError');
                errorElement.innerText = 'Email or User Name is required';
                errorElement.style.display = 'block'; // Show the error message
                isValid = false;
            }

            if (Identifier.value.trim() === '' || Identifier.value.trim().length < 5) {
                const errorElement = document.getElementById('IdentifierError');
                errorElement.innerText = 'Email or User Name is required and must be at least more then 5 characters';
                errorElement.style.display = 'block'; // Show the error message
                isValid = false;
            }


            if (Password.value.length <= 6) {
                const errorElement = document.getElementById('PasswordLoginError');
                errorElement.innerText = 'Password must be longer than six characters';
                errorElement.style.display = 'block'; // Show the error message
                isValid = false;
            }
            if (Password.value.length >= 10) {
                const errorElement = document.getElementById('PasswordLoginError');
                errorElement.innerText = 'Password must be shorter than ten characters';
                errorElement.style.display = 'block'; // Show the error message
                isValid = false;
            }
            if (Password.value.toLowerCase() === 'password') {
                const errorElement = document.getElementById('PasswordLoginError');
                errorElement.innerText = 'Password cannot be "password"';
                errorElement.style.display = 'block'; // Show the error message
                isValid = false;
            }
        }

        return isValid; // Return the form validity
    }

    const registerForm = document.getElementById('registerForm');
    if (registerForm) {
        registerForm.addEventListener('submit', (e) => {
            if (!validateForm('register')) {
                e.preventDefault(); // Prevent form submission if validation fails
            }
        });
    }

    const loginForm = document.getElementById('loginForm');
    if (loginForm) {
        loginForm.addEventListener('submit', (e) => {
            if (!validateForm('login')) {
                e.preventDefault(); // Prevent form submission if validation fails
            }
        });
    }
});