import http from 'k6/http';
import { sleep, check } from 'k6';
import { Counter } from 'k6/metrics';

export const requests = new Counter('http_reqs');

const reqHeader = {
   headers: { 'Content-Type': 'application/json', "x-api-key": `testing` },
}

export const options = {
   scenarios: {
      sampleTest: {
         executor: 'constant-arrival-rate',
         exec: 'sampleTest',
         duration: '10s',
         rate: 1,
         timeUnit: `1s`,
         preAllocatedVUs: 1,
         maxVUs: 1,
      }
   }
};

export function sampleTest() {
   // read from environment variable

   let appUrl = __ENV.AppUrl + "/Appointment?status=failed";

   let res = http.get(appUrl, reqHeader);
   let checkRes = check(res, {
      'status is 200': (r) => r.status === 200,
   });
}