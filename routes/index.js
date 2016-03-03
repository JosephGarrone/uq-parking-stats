var express = require('express');
var mysql = require('mysql');
var router = express.Router();
var conn = mysql.createConnection({
    host: 'localhost',
    user: 'uq_parking',
    password: 'uq_parking',
    database: 'uq_parking'
})
conn.connect();

/* GET home page. */
router.get('/', function (req, res, next) {
    conn.query('SELECT * FROM `car_parks`', function(err, rows, fields) {
       if (err) throw err;
       console.log(JSON.stringify(rows));
       
        res.render('index', {
            carParks: rows
        });
    });
});

/* GET data */
router.get('/:carpark/:date', function (req, res, next) {
    conn.query('SELECT * FROM `car_park_info` WHERE `car_park` = ? AND DATE(`time`) = STR_TO_DATE(?, "%Y-%m-%d") GROUP BY UNIX_TIMESTAMP(`time`) DIV 300', [req.params.carpark, req.params.date], function(err, rows, fields) {
        res.send(rows);
    });
})

module.exports = router;
