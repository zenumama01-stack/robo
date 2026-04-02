import mssql from 'mssql';
import { dbDatabase, dbHost, dbPassword, dbPort, dbUsername } from '../config';
const SQLConnectionPool = new mssql.ConnectionPool(config);
export default SQLConnectionPool;
import { dbDatabase, dbHost, dbPassword, dbPort, dbUsername } from './config';
    requestTimeout: 300000, // some things run a long time, so we need to set this to a high value, this is 300,000 milliseconds or 5 minutes
        encrypt: true, // Use encryption
        trustServerCertificate: true // For development/testing
