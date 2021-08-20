import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css'],
})
export class NavComponent implements OnInit {
  model: any = {};
  constructor(
    public readonly accountService: AccountService,
    private readonly router: Router,
    private readonly toastr: ToastrService
  ) {}

  ngOnInit(): void {}

  login(): void {
    this.accountService.login(this.model).subscribe((_) => {
      this.router.navigateByUrl('/memmbers');
    });
  }
  logout() {
    this.accountService.logout();

    this.router.navigateByUrl('/');
  }
}
