/*
Copyright 2012, Marcus McKinnon, E.J. Wilburn, Kevin Williams
This program is distributed under the terms of the GNU General Public License.

This file is part of Palaver.

Palaver is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 2 of the License, or
(at your option) any later version.

Palaver is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Palaver.  If not, see <http://www.gnu.org/licenses/>.
*/

function addLayouts() {
    myLayout = $('.page').layout({
        west__size: 400,
        north__size: 70,
        south__size: 30,
        south__resizable: false,
        north__resizable: false
    });
    // THEME SWITCHER
    addThemeSwitcher('.ui-layout-north', { top: '25px', right: '5px' });
    // if a new theme is applied, it could change the height of some content,
    // so call resizeAll to 'correct' any header/footer heights affected
    // NOTE: this is only necessary because we are changing CSS *AFTER LOADING* using themeSwitcher
    setTimeout(myLayout.resizeAll, 1000); /* allow time for browser to re-render with new theme */
}
