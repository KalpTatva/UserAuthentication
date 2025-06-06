$(document).ready(function() {
    $.ajax({
         url: '/Dashboard/GetAllUserDetails',
         type: 'GET',
         success: function(data) {
             $('#UserContent').html(data);
         },
         error: function(xhr, status, error) {
             console.error('Error fetching user details:', error);
             $('#UserContent').html('<p class="text-danger">Failed to load user details.</p>');
         }
     });
 });