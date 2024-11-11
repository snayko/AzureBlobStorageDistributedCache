import http from 'k6/http';
import { sleep } from 'k6';

// Options to configure test
export let options = {
  stages: [
    { duration: '1m', target: 50 },   // Ramp up to 50 VUs over 1 minute
    { duration: '5m', target: 50 },   // Stay at 50 VUs for 5 minutes
    { duration: '1m', target: 0 },    // Ramp down to 0 VUs
  ],
};

// Test logic
export default function () {
  // Generate a unique userId for each VU
  let userId = `user${__VU}`;

  let res = http.get(`http://webapptestazureblobdistributedcache:8080/WeatherForecast?userId=${userId}`);
  if (res.status !== 200) {
    console.error(`Failed with status ${res.status}`);
  }
  sleep(1); // Wait 1 second between requests
}