# DATABASE GENERATOR

This db generator was created and tested on mysql database, which was created using auto scripts of MYSQL Workbench.

## Brief explanation

Current program based on mysql queries, which inserts automatically all data in all tables of db, but for correct working db should meet some rquirements:

**All public keys in database should be autoincrement**
**All data should be added using arrays**

The second requirement will be change in nearly future, if somebody would be able to upgrade code. 

## Usage

For executing generator you will be requested to set password and login of your db user. After that 
program execute Generator funct, in which user specify by code a table, were to insert data, and multiple arrays arguments which represents input data.

#!!!Note!!!

Place of all arrays with data should be correctly setted as in your db.

>Example
>
>`Columns form table customer`
>
> `1:Name, 2:Email, Details;`
>
>`Input data`
>
>`1:Data_Name, 2:Data_Emails, Data_Details`

## About repo

This code requires refaactoring and this generator can't manipulate with every data type of mysql, so that I would be glad, if you would like to modify this repo.
**Good luck deBUGgers!**