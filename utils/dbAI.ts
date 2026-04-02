  requestTimeout: 45000,
    trustServerCertificate: true
const pool = new sql.ConnectionPool(config);
export default pool;