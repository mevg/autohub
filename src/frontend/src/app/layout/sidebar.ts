import { Component, input, output } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

export interface NavItem {
  label: string;
  route: string;
}

@Component({
  selector: 'app-sidebar',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './sidebar.html',
})
export class Sidebar {
  appName = input.required<string>();
  logoUrl = input.required<string>();
  version = input.required<string>();
  items = input<NavItem[]>([{ label: 'Bienvenida', route: '/bienvenida' }]);
  open = input(false);
  closed = output<void>();
}
