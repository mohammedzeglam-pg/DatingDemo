import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-test-errors',
  templateUrl: './test-errors.component.html',
  styleUrls: ['./test-errors.component.css'],
})
export class TestErrorsComponent implements OnInit {
  constructor(private readonly http: HttpClient) {}
  validationErrors: string[] = [];
  ngOnInit(): void {}
  get404Error() {
    this.http.get('/api/buggy/not-found').subscribe(
      (response) => console.log(response),
      (error) => console.log(error)
    );
  }

  get400Error() {
    this.http.get('/api/buggy/bad-request').subscribe(
      (response) => console.log(response),
      (error) => console.log(error)
    );
  }

  get500Error() {
    this.http.get('/api/buggy/server-error').subscribe(
      (response) => console.log(response),
      (error) => console.log(error)
    );
  }

  get401Error() {
    this.http.get('/api/buggy/auth').subscribe(
      (response) => console.log(response),
      (error) => console.log(error)
    );
  }

  getValidtionError() {
    this.http.post('/api/account/register', {}).subscribe(
      (response) => console.log(response),
      (error) => {
        console.log(error);
        this.validationErrors = error;
      }
    );
  }
}
