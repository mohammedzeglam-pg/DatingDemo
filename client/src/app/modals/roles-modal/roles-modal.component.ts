import { Component, Input, OnInit } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { Subject } from 'rxjs';
import { User } from 'src/app/_models/user';

@Component({
  selector: 'app-roles-modal',
  templateUrl: './roles-modal.component.html',
  styleUrls: ['./roles-modal.component.css'],
})
export class RolesModalComponent implements OnInit {
  @Input() updateSelectedRoles = new Subject();
  user: User;
  roles: any[];
  constructor(public bsModalRef: BsModalRef) {}
  ngOnInit(): void {}
  updateRoles(): void {
    this.updateSelectedRoles.next(this.roles);
    this.bsModalRef.hide();
  }
}
