# Azure Blob Storage Distributed Cache and tests(K6) comparison with Redis 

Implementation of a distributed cache using Azure Blob Storage. 
This project aims to compare the performance of cache by using simple Blob Storage and Redis.

---

## Table of Contents

- [Overview](#overview)
- [Setup](#setup)
- [Testing](#testing)
    - [Local Testing](#local-testing)
    - [Cloud Testing](#cloud-testing)
- [Results](#results)
    - [Localhost Results](#localhost-results)
    - [Cloud Results](#cloud-results)
- [Conclusion](#conclusion)

---

## Overview

---

## Setup

---

## Testing
Azure Blob Storage plan used:
- StorageV2 (general purpose v2), Locally-redundant storage (LRS) with location: germanywestcentral
Azure Redis Cache plan used:
- Basic 250 MB with location: germanywestcentral

### Local Testing
<div style="background-color: #f9f9f9; padding: 20px; border-radius: 8px; border: 1px solid #ddd; font-family: monospace;">
Run in docker-compose environment on Macbook Pro M1 and with Azure blob storage and redis as external cache (outside of azure cluster). 
</div>

#### Blob Storage Cache

| Metric                          | Value                    |
|---------------------------------|--------------------------|
| **Data Received**               | 2.0 GB (4.7 MB/s)       |
| **Data Sent**                   | 730 kB (1.7 kB/s)       |
| **HTTP Request Blocked**        | avg=10.15µs, min=0s, med=4.2µs, max=2.49ms, p(90)=9.83µs, p(95)=14.04µs |
| **HTTP Request Connecting**     | avg=3.21µs, min=0s, med=0s, max=2.15ms, p(90)=0s, p(95)=0s             |
| <span style="background-color: #ffeb3b; font-weight: bold;">**HTTP Request Duration**</span> | <span style="background-color: #ffeb3b; font-weight: bold;">avg=2.33s, min=0s, med=1.89s, max=1m0s, p(90)=2.89s, p(95)=5.29s</span> |
| **Expected Response (true)**    | avg=2.32s, min=61.02ms, med=1.89s, max=55.49s, p(90)=2.89s, p(95)=5.28s |
| **HTTP Request Failed**         | 0.05% (3 out of 5421)   |
| **HTTP Request Receiving**      | avg=129.96ms, min=0s, med=106.93ms, max=2.63s, p(90)=258.49ms, p(95)=344.97ms |
| **HTTP Request Sending**        | avg=18.83µs, min=0s, med=13.16µs, max=4.98ms, p(90)=29.25µs, p(95)=41.7µs |
| **HTTP TLS Handshaking**        | avg=0s, min=0s, med=0s, max=0s, p(90)=0s, p(95)=0s                     |
| **HTTP Request Waiting**        | avg=2.2s, min=0s, med=1.77s, max=1m0s, p(90)=2.69s, p(95)=4.94s        |
| **HTTP Requests**               | 5421 (12.88/s)          |
| **Iteration Duration**          | avg=3.33s, min=1s, med=2.89s, max=1m1s, p(90)=3.89s, p(95)=6.29s       |
| **Iterations**                  | 5421 (12.88/s)          |
| **Virtual Users (VUs)**         | 1 (min=1, max=50)       |
| **Max VUs**                     | 50                      |


#### Redis Cache

* Avg Cache GET request duration: ± 1.1 s
* Avg Cache SAVE request duration: ± 0.6 s
* Avg Fake Data generation duration: 0.31 - 4.1 s

| Metric                                | Value                                                                                                                              |
|---------------------------------------|------------------------------------------------------------------------------------------------------------------------------------|
| **Data Received**                     | 2.4 GB (5.8 MB/s)                                                                                                                  |
| **Data Sent**                         | 853 kB (2.0 kB/s)                                                                                                                  |
| **HTTP Request Blocked**              | avg=15.34µs, min=0s, med=6.58µs, max=8.03ms, p(90)=17.04µs, p(95)=22.98µs                                                          |
| **HTTP Request Connecting**           | avg=3.88µs, min=0s, med=0s, max=1.67ms, p(90)=0s, p(95)=0s                                                                         |
|  <span style="background-color: #ffeb3b; font-weight: bold;">**HTTP Request Duration**</span> | <span style="background-color: #ffeb3b; font-weight: bold;">avg=1.85s, min=0s, med=1.83s, max=21.87s, p(90)=2.7s, p(95)=3s </span> |
| **Expected Response Duration**        | avg=1.85s, min=14.73ms, med=1.83s, max=21.87s, p(90)=2.7s, p(95)=3s                                                                |
| **HTTP Requests Failed**              | 0.04% (3 out of 6330)                                                                                                              |
| **HTTP Request Receiving**            | avg=124.52ms, min=0s, med=78.41ms, max=3.41s, p(90)=259.26ms, p(95)=369.44ms                                                       |
| **HTTP Request Sending**              | avg=26.27µs, min=0s, med=18.04µs, max=1.97ms, p(90)=48.91µs, p(95)=66.46µs                                                         |
| **HTTP Request TLS Handshaking**      | avg=0s, min=0s, med=0s, max=0s, p(90)=0s, p(95)=0s                                                                                 |
| **HTTP Request Waiting**              | avg=1.72s, min=0s, med=1.72s, max=21.57s, p(90)=2.56s, p(95)=2.78s                                                                 |
| **Total HTTP Requests**               | 6330 (15.06/s)                                                                                                                     |
| **Iteration Duration**                | avg=2.85s, min=1s, med=2.83s, max=22.87s, p(90)=3.7s, p(95)=4s                                                                     |
| **Total Iterations**                  | 6330 (15.06/s)                                                                                                                     |
| **Virtual Users (VUs)**               | 1 (min=1, max=50)                                                                                                                  |
| **Max VUs**                           | 50 (min=50, max=50)                                                                                                                |


