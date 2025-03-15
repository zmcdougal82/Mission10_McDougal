import React, { useState, useEffect } from 'react';
import axios from 'axios';

function TestDatabase() {
  const [testResult, setTestResult] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchTestResult = async () => {
      try {
        console.log('Testing database connection...');
        const response = await axios.get('/api/test');
        console.log('Test response:', response.data);
        setTestResult(response.data);
        setLoading(false);
      } catch (err) {
        console.error('Error testing database:', err);
        
        let errorMessage = 'Error testing database. Please check the console for details.';
        
        if (err.response) {
          console.error('Error response data:', err.response.data);
          console.error('Error response status:', err.response.status);
          errorMessage = `Server error: ${err.response.status} - ${JSON.stringify(err.response.data)}`;
        } else if (err.request) {
          console.error('Error request:', err.request);
          errorMessage = 'No response received from server. Please check your connection.';
        } else {
          console.error('Error message:', err.message);
          errorMessage = `Error: ${err.message}`;
        }
        
        setError(errorMessage);
        setLoading(false);
      }
    };

    fetchTestResult();
  }, []);

  if (loading) {
    return <div>Testing database connection...</div>;
  }

  if (error) {
    return <div className="alert alert-danger">{error}</div>;
  }

  return (
    <div>
      <h2>Database Connection Test</h2>
      <pre className="bg-light p-3 mt-3" style={{ maxHeight: '500px', overflow: 'auto' }}>
        {JSON.stringify(testResult, null, 2)}
      </pre>
    </div>
  );
}

export default TestDatabase;
