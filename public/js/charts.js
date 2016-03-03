google.charts.load('current', { 'packages': ['line'] })
google.charts.setOnLoadCallback(drawChart);

$('#car-park').change(drawChart);
$('#graph-type').change(drawChart);

function drawChart() {
    var carPark = $('#car-park option:selected').text();
    var carParkId = $('#car-park').val();
    
    var data = new google.visualization.DataTable();
    data.addColumn('datetime', 'Time');
    data.addColumn('number', carPark);
    
    $.ajax('/' + carParkId).done(function(json) {
        if (json.length == 0) {
            return;
        }
        
        for (var i = 0; i < json.length; i++) {
            data.addRow([new Date(new Date(json[i].time).toString()), json[i].available])
        }
        
        var options = {
            height: 500,
            hAxis: {
                viewWindow: {
                    min: new Date(new Date(json[0].time).toString()),
                    max: new Date(new Date(json[json.length - 1].time).toString())
                },
                gridlines: {
                    count: -1,
                    units: {
                        days: { format: ['MMM dd'] },
                        hours: { format: ['HH:mm', 'ha'] },
                    }
                },
                minorGridlines: {
                    units: {
                        hours: { format: ['hh:mm:ss a', 'ha'] },
                        minutes: { format: ['HH:mm a Z', ':mm'] }
                    }
                }
            }
        };

        var chart = new google.charts.Line(document.getElementById('line_top_x'));

        chart.draw(data, options);
    }).fail(function() {
        
    }).always(function() {
        
    })
}