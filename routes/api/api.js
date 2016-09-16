var express = require('express');
var mysql = require('mysql');
var router = express.Router();
var conn = mysql.createPool({
    connectionLimit: 10,
    host: 'localhost',
    user: 'uq_parking',
    password: 'uq_parking',
    database: 'uq_parking',
    timezone: 'utc'
})

/* GET carparks */
router.get('/carparks', function( req, res, next) {
    conn.query('SELECT * FROM `car_parks`', function(err, rows, fields) {
        res.send(rows);
    });
});

/* GET data */
router.get('/:carpark/:date', function (req, res, next) {
    // Set the hours, minutes and seconds
    req.params.date = req.params.date + ' 00:00:00';

    // Build query
    var qry, qryParams;
    if (req.params.carpark == "all") {
        
        qry = `SELECT SUM(available) as available, time
                FROM (
                    SELECT car_park, available, FROM_UNIXTIME((UNIX_TIMESTAMP(time) DIV 300) * 300) AS time
                    FROM car_park_info
                    LEFT JOIN car_parks ON (car_park_info.car_park = car_parks.id)
                    WHERE time >= DATE_SUB(?, INTERVAL 10 HOUR)
                        AND time < DATE_ADD(?, INTERVAL 14 HOUR)
                    GROUP BY car_park_info.car_park, UNIX_TIMESTAMP(time) DIV 300
                ) AS t
                GROUP BY time`;

        qryParams = [req.params.date, req.params.date];
        
    } else if (req.params.carpark == "casual" || req.params.carpark == "permit") {
        
        qry = `SELECT SUM(available) as available, time
                FROM (
                    SELECT car_park, available, FROM_UNIXTIME((UNIX_TIMESTAMP(time) DIV 300) * 300) AS time
                    FROM car_park_info
                    LEFT JOIN car_parks ON (car_park_info.car_park = car_parks.id)
                    WHERE time >= DATE_SUB(?, INTERVAL 10 HOUR)
                        AND time < DATE_ADD(?, INTERVAL 14 HOUR)
                        AND car_parks.casual = ?
                    GROUP BY car_park_info.car_park, UNIX_TIMESTAMP(time) DIV 300
                ) AS t
                GROUP BY time`;
        
        if (req.params.carpark == "casual") {
            qryParams = [req.params.date, req.params.date, 1];
        } else {
            qryParams = [req.params.date, req.params.date, 0];
        }
        
    } else {
        qry = `SELECT available, FROM_UNIXTIME((UNIX_TIMESTAMP(time) DIV 300) * 300) AS time
                FROM car_park_info
                WHERE time >= DATE_SUB(?, INTERVAL 10 HOUR)
                    AND time < DATE_ADD(?, INTERVAL 14 HOUR)
                    AND car_park = ?
                GROUP BY UNIX_TIMESTAMP(time) DIV 300`;
        
        qryParams = [req.params.date, req.params.date, req.params.carpark];
    }
    
    // Execute
    console.log(qryParams);
    conn.query(qry, qryParams, function(err, rows, fields) {
        res.send(rows);
    });
})

module.exports = router;
