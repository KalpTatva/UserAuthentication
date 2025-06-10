$(document).ready(function() {
    var openEditUserModal = new bootstrap.Modal(document.getElementById('EditUserModal'), { 
        keyboard: false,
        backdrop: 'static'
    });

    var openDeleteUserModal = new bootstrap.Modal(document.getElementById('DeleteUserModal'), {
        keyboard: false,
        backdrop: 'static'
    });

    function fetchUsers(){
        $('.loader').show(); 
        $.ajax({
            url: '/Dashboard/GetAllUserDetails',
            type: 'GET',
            success: function(data) {
            $('#UserContent').html(data);
            $('.loader').hide(); 
            },
            error: function(xhr, status, error) {
            console.error('Error fetching user details:', error);
            $('#UserContent').html('<p class="text-danger">Failed to load user details.</p>');
            $('.loader').hide(); 
            }
        });
        
    }


    $(document).on('click', '.edit-user-btn', function(){
        var userId = $(this).data('user-id');
        // console.log('Edit button clicked for user ID:', userId);
        

        $.ajax({
            url: '/Dashboard/EditUser',
            type: 'GET',
            data: { userId: userId },
            success: function(data) {
                $('#Edit-user-data').html(data);
                openEditUserModal.show();
            },
            error: function(xhr, status, error) {
                console.error('Error fetching user details for edit:', error);
                $('#UserContent').html('<p class="text-danger">Failed to load user details for editing.</p>');
            }
        });
    });

    $(document).on('click','.delete-user-btn',function(){
        var userId = $(this).data('user-id');
        console.log('Delete button clicked for user ID:', userId);
        $('#DeleteUserId').val(userId);
        openDeleteUserModal.show();
    })

    $(document).on('submit','#DeleteUserForm', function(e){
        e.preventDefault();
        var userId = $('#DeleteUserId').val();
        console.log('Submitting delete for user ID:', userId);
        
        $.ajax({
            url: '/Dashboard/DeleteUser',
            type: 'PUT',
            data: { userId: userId },
            success: function(response) {
                if (response.success) {
                    toastr.success(response.message, 'Success', { timeOut: 6000 });
                    openDeleteUserModal.hide();
                    fetchUsers();
                } else {
                    toastr.error(response.message, 'Error', { timeOut: 6000 });
                }
            },
            error: function(xhr, status, error) {
                console.error('Error deleting user:', error);
                toastr.error('Failed to delete user.', 'Error', { timeOut: 6000 });
            }
        });
    })


    $(document).on('submit', '#EditUserForm', function(e) {
        e.preventDefault();
        var formData = $(this).serialize();
        console.log('Submitting edit user form:', formData);
        
        $.ajax({
            url: '/Dashboard/EditUser',
            type: 'PUT',
            data: formData,
            success: function(response) {
                if (response.success) {
                    toastr.success(response.message, 'Success', { timeOut: 6000 });
                    openEditUserModal.hide();
                    fetchUsers();
                } else {
                    toastr.error(response.message, 'Error', { timeOut: 6000 });
                }
            },
            error: function(xhr, status, error) {
                console.error('Error editing user:', error);
                toastr.error('Failed to edit user.', 'Error', { timeOut: 6000 });
            }
        });
    });


    $(document).on('input', '.DateOfBirthEdit', function () {
        var dob = new Date($(this).val());
        var today = new Date();
        var age = today.getFullYear() - dob.getFullYear();
        var m = today.getMonth() - dob.getMonth();
        if (m < 0 || (m === 0 && today.getDate() < dob.getDate())) {
            age--;
        }
        $('.AgeCalculation2').val(age);
        if(age < 18){
            $('.DOBValidationSpan2').text('Age must be at least 18 years.');
            $('.EditUserBtn').prop('disabled', true);
        } else {
            $('.DOBValidationSpan2').text('');
            $('.EditUserBtn').prop('disabled', false);
        }
    });
    
    fetchUsers();
 });