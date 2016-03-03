google.charts.load('current', { 'packages': ['line'] })
google.charts.setOnLoadCallback(drawChart);

$('#car-park').change(drawChart);
$('#graph-type').change(drawChart);
$('#view-date').change(drawChart);

function drawChart() {
    var carPark = $('#car-park option:selected').text();
    var carParkId = $('#car-park').val();
    
    var date = $('#view-date').pickadate('picker').get('select', 'yyyy-mm-dd');
    
    var data = new google.visualization.DataTable();
    data.addColumn('datetime', 'Time');
    data.addColumn('number', carPark);
    
    $.ajax('/' + carParkId + '/' + date).done(function(json) {
        if (json.length == 0) {
            return;
        }
        
        for (var i = 0; i < json.length; i++) {
            data.addRow([new Date(new Date(json[i].time).toString()), json[i].available])
        }
        
        var options = {
            height: 500,
            width: $("#graph").outerWidth(),
            legend: {
                position: 'none'
            },
            vAxis: {
                title: 'Parks'
            },
            hAxis: {
                title: 'Time'
            }
        };

        var chart = new google.charts.Line(document.getElementById('graph'));

        chart.draw(data, options);
    }).fail(function() {
        
    }).always(function() {
        
    })
}