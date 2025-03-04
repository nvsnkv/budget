import { Component } from '@angular/core';
import { TuiGroup } from '@taiga-ui/core';
import { UserService } from '../auth/user.service';
import { Observable, map } from 'rxjs';
import { AsyncPipe, NgIf } from '@angular/common';

@Component({
  selector: 'app-index',
  imports: [TuiGroup, AsyncPipe, NgIf],
  templateUrl: './index.component.html',
  styleUrl: './index.component.less'
})
export class IndexComponent {

  isAuthenticated$: Observable<boolean>;

  constructor(user: UserService) {
    this.isAuthenticated$ = user.current$.pipe(map(user => user.isAuthenticated));
  }
}
