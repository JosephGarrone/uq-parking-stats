(function($){
  $(function(){

    $('.button-collapse').sideNav();
    
    $(document).ready(function () {
        $('select').material_select();
        $('.datepicker').pickadate({
            selectMonths: true, // Creates a dropdown to control month
            selectYears: 15 // Creates a dropdown of 15 years to control year
        });
        $('.datepicker').each(function(index) {
            var picker = $(this).pickadate('picker');
            picker.set('select', new Date());
        })
    });

  });
})(jQuery);