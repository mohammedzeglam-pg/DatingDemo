import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
})
export class RegisterComponent implements OnInit {
  @Output() cancelRegister = new EventEmitter<boolean>();
  model: any = {};
  constructor(
    private readonly accountService: AccountService,
    private readonly toastr: ToastrService
  ) {}

  ngOnInit(): void {}
  register() {
    this.accountService.register(this.model).subscribe(
      (resp) => {
        console.log(resp);
        this.cancel();
      },
      (error) => {
        console.log(error);
        this.toastr.error(error.error);
      }
    );
  }
  cancel() {
    this.cancelRegister.emit(false);
  }
}
