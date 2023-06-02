import { View } from 'project/view';

export class UrlUtil {

    /**
     * Returns the current page group, i.e. "/Admin"", "/projects", etc.
     * "/projects" from "/projects/1?view=0", etc.
     * @returns {string}
     */
    static GetCurrentPage(): string {
        const path = window.location.pathname.split('/');
        return path[1];
    }

    /**
     * Returns the current page group, i.e. "/Admin"", "/projects", etc.
     * "/projects" from "/projects/1?view=0", etc.
     * @returns {string}
     */
    static GetCurrentPath(): string {
        const path = window.location.pathname.split('/');
        if (path.length < 2) {
            return null;
        }

        return path[2];
    }

    /**
     * Gets the current view when inside "/projects".
     * @returns {View}
     */
    static GetCurrentView(): View {
        const queryString = window.location.search;
        const urlParams = new URLSearchParams(queryString);
        const view = urlParams.get("view") as unknown as number as View;
        return view;
    }

    /**
     * Returns wether or not the user is in the correct page and view.
     * @param {string} page
     * @param {View} view
     * @returns {boolean}
     */
    static IsCorrectPage(page: string, view: View): boolean {
        const currentPage = this.GetCurrentPage();
        if (currentPage != page) {
            return false;
        }

        const currentView = this.GetCurrentView();
        if (currentView != view) {
            return false;
        }

        return true;
    }

    static IsCorrectPageAndPath(page: string, path: string): boolean {
        const currentPage = this.GetCurrentPage();
        if (currentPage != page) {
            return false;
        }

        return true;
    }
}
