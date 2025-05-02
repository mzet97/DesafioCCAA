import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
    scenarios: {
        ramp_up: {
            executor: 'ramping-vus',
            startVUs: 0,
            stages: [
                { duration: '1m', target: 1000 },
                { duration: '2m', target: 10000 },
                { duration: '1m', target: 1000 },
            ],
        },
    },
};

export default function () {
    let res = http.get('http://localhost:5002/cached-endpoint');

    check(res, {
        'status é 200': (r) => r.status === 200,
        'tempo de resposta < 400ms': (r) => r.timings.duration < 400,
    });

    sleep(1);
}
