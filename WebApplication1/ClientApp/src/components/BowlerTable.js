import React, { useState, useEffect } from 'react';
import axios from 'axios';

function BowlerTable() {
  const [bowlers, setBowlers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchBowlers = async () => {
      try {
        console.log('Fetching bowlers from API...');
        const response = await axios.get('/api/bowlers');
        
        // Detailed logging of the response (for console only)
        console.log('API response status:', response.status);
        console.log('API response headers:', response.headers);
        console.log('API response data:', response.data);
        console.log('API response type:', typeof response.data);
        console.log('Is array?', Array.isArray(response.data));
        
        // Always ensure bowlers is an array
        let bowlersArray = [];
        
        if (response.data === null || response.data === undefined) {
          console.error('Response data is null or undefined');
          setError('Received null or undefined data from the server.');
        }
        // Check if the response data is an array
        else if (Array.isArray(response.data)) {
          console.log('Setting bowlers from array data');
          bowlersArray = response.data;
        } 
        // Check if the response data is wrapped in a value property
        else if (response.data && response.data.value && Array.isArray(response.data.value)) {
          console.log('Setting bowlers from response.data.value');
          bowlersArray = response.data.value;
        }
        // Check if the response data is a JSON object with a result property
        else if (response.data && response.data.result && Array.isArray(response.data.result)) {
          console.log('Setting bowlers from response.data.result');
          bowlersArray = response.data.result;
        }
        // If it's an object with numeric keys, it might be an object that should be an array
        else if (typeof response.data === 'object' && response.data !== null) {
          console.log('Attempting to convert object to array');
          const values = Object.values(response.data);
          if (values.length > 0) {
            console.log('Setting bowlers from Object.values');
            bowlersArray = values;
          } else {
            console.error('Object has no values');
            setError('Received an empty object from the server.');
          }
        } 
        else {
          console.error('Unexpected data format:', response.data);
          setError('Received data in an unexpected format. Please check the console for details.');
        }
        
        // Final safety check to ensure bowlersArray is actually an array
        if (!Array.isArray(bowlersArray)) {
          console.error('bowlersArray is not an array:', bowlersArray);
          bowlersArray = [];
          if (!error) {
            setError('Failed to process data from server. Expected an array but got something else.');
          }
        }
        
        // Log the first item in the array if it exists
        if (bowlersArray.length > 0) {
          console.log('First bowler in array:', bowlersArray[0]);
          console.log('First bowler properties:', Object.keys(bowlersArray[0]));
        }
        
        // Set the bowlers state with our guaranteed array
        setBowlers(bowlersArray);
        
        setLoading(false);
      } catch (err) {
        console.error('Error fetching data:', err);
        
        let errorMessage = 'Error fetching bowler data. Please try again later.';
        
        if (err.response) {
          // The request was made and the server responded with a status code
          // that falls out of the range of 2xx
          console.error('Error response data:', err.response.data);
          console.error('Error response status:', err.response.status);
          errorMessage = `Server error: ${err.response.status} - ${err.response.data.title || err.response.data}`;
        } else if (err.request) {
          // The request was made but no response was received
          console.error('Error request:', err.request);
          errorMessage = 'No response received from server. Please check your connection.';
        } else {
          // Something happened in setting up the request that triggered an Error
          console.error('Error message:', err.message);
          errorMessage = `Error: ${err.message}`;
        }
        
        setError(errorMessage);
        setLoading(false);
      }
    };

    fetchBowlers();
  }, []);

  if (loading) {
    return <div>Loading bowler data...</div>;
  }

  if (error) {
    return <div className="alert alert-danger">{error}</div>;
  }

  return (
    <div className="table-responsive">
      <table className="table">
        <thead>
          <tr>
            <th>Name</th>
            <th>Team</th>
            <th>Address</th>
            <th>City</th>
            <th>State</th>
            <th>Zip</th>
            <th>Phone</th>
          </tr>
        </thead>
        <tbody>
          {bowlers.length > 0 ? (
            bowlers.map((bowler) => (
              <tr key={bowler.bowlerId || Math.random()}>
                <td>
                  {bowler.bowlerFirstName || 'Unknown'}{' '}
                  {bowler.bowlerMiddleInit ? bowler.bowlerMiddleInit + '. ' : ''}
                  {bowler.bowlerLastName || 'Unknown'}
                </td>
                <td>{bowler.teamName || 'N/A'}</td>
                <td>{bowler.bowlerAddress || 'N/A'}</td>
                <td>{bowler.bowlerCity || 'N/A'}</td>
                <td>{bowler.bowlerState || 'N/A'}</td>
                <td>{bowler.bowlerZip || 'N/A'}</td>
                <td>{bowler.bowlerPhoneNumber || 'N/A'}</td>
              </tr>
            ))
          ) : (
            <tr>
              <td colSpan="7">No bowlers found or data is not in the expected format.</td>
            </tr>
          )}
        </tbody>
      </table>
      {bowlers.length === 0 && !error && !loading && (
        <div className="alert alert-info">No bowlers found.</div>
      )}
    </div>
  );
}

export default BowlerTable;
