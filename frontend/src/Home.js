import { useState } from 'react';
import logo from './logo.svg';
import './Home.css';

function Home() {
  const [formData, setFormData] = useState({
    username: '',
    email: '',
    phoneNumber: '',
    password: '',
    confirmPassword: '',
  });

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData({
      ...formData,
      [name]: value,
    });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    console.log('Attempting to submit unsecure data:', formData);

    try {
      // This fetch call will send data without any client-side validation
      // Normally, input validation should be done on the client side 
      // as well as the server side.
      const response = await fetch('http://localhost:5000/signup', { // FIX THIS LINE
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(formData),
      });

      const data = await response.json();

      if (response.ok) {
        alert('Sign-up attempt sent (unsecurely processed): ' + data.message);
        setFormData({
          username: '',
          email: '',
          phoneNumber: '',
          password: '',
          confirmPassword: '',
        });
      } else {
        alert('Sign-up failed (unsecure processing issue): ' + (data.message || 'Unknown error'));
      }
    } catch (error) {
      console.error('Network or server error:', error);
      alert('Could not connect to the backend. Is it running?');
    }
  };

  return (
    <div className="home-container">

      <img src={logo} className="Home-logo" alt="logo" />
      <p className="Home-title">
        Secure Sign-Up Demonstration
      </p>

      <h2>Sign Up (Unsecure)</h2>
      <form onSubmit={handleSubmit} className="signup-form">
        <div className="form-group">
          <label htmlFor="username">Username:</label>
          <input
            type="text"
            id="username"
            name="username"
            value={formData.username}
            onChange={handleChange}
            placeholder="Enter username"
          />
        </div>

        <div className="form-group">
          <label htmlFor="email">Email:</label>
          <input
            type="text"
            id="email"
            name="email"
            value={formData.email}
            onChange={handleChange}
            placeholder="Enter email"
          />
        </div>

        <div className="form-group">
          <label htmlFor="phoneNumber">Phone Number:</label>
          <input
            type="text" 
            id="phoneNumber"
            name="phoneNumber"
            value={formData.phoneNumber}
            onChange={handleChange}
            placeholder="Enter phone number"
          />
        </div>

        <div className="form-group">
          <label htmlFor="password">Password:</label>
          <input
            type="password"
            id="password"
            name="password"
            value={formData.password}
            onChange={handleChange}
            placeholder="Enter password"
          />
        </div>

        <div className="form-group">
          <label htmlFor="confirmPassword">Confirm Password:</label>
          <input
            type="password"
            id="confirmPassword"
            name="confirmPassword"
            value={formData.confirmPassword}
            onChange={handleChange}
            placeholder="Confirm password"
          />
        </div>

        <button type="submit" className="submit-button">Sign Up</button>
      </form>
    </div>
  );
}

export default Home;