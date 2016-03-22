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
       
        res.render('index', {
            carParks: rows
        });
    });
});

module.exports = router;
