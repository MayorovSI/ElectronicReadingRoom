window.viewerInterop = (() => {
    let activeViewer = null;
    let coverObserver = null;
    let coverQueue = Promise.resolve();

    function showCoverFallback(element, title, format) {
        const fallback = document.createElement('div');
        fallback.className = 'doc-cover-fallback';
        fallback.textContent = title || format.toUpperCase();
        element.replaceChildren(fallback);
    }

    async function renderPdfCover(element, url) {
        const pdfjsLib = await import('https://cdnjs.cloudflare.com/ajax/libs/pdf.js/4.4.168/pdf.min.mjs');
        pdfjsLib.GlobalWorkerOptions.workerSrc = 'https://cdnjs.cloudflare.com/ajax/libs/pdf.js/4.4.168/pdf.worker.min.mjs';
        const documentProxy = await pdfjsLib.getDocument(url).promise;
        const firstPage = await documentProxy.getPage(1);
        const baseViewport = firstPage.getViewport({ scale: 1 });
        const scale = Math.min(element.clientWidth / baseViewport.width, element.clientHeight / baseViewport.height);
        const viewport = firstPage.getViewport({ scale: Math.max(scale, 0.1) });
        const canvas = document.createElement('canvas');
        canvas.className = 'doc-cover-page';
        canvas.width = viewport.width;
        canvas.height = viewport.height;
        await firstPage.render({ canvasContext: canvas.getContext('2d'), viewport }).promise;
        element.replaceChildren(canvas);
        await documentProxy.destroy();
    }

    async function renderEpubCover(element, url) {
        const book = window.ePub(url, { openAs: 'epub' });
        await book.ready;
        const coverUrl = await book.coverUrl();
        if (coverUrl) {
            const image = new Image();
            image.alt = 'Обложка документа';
            image.src = coverUrl;
            await image.decode();
            element.replaceChildren(image);
            return;
        }

        const rendition = book.renderTo(element, {
            width: '100%',
            height: '100%',
            flow: 'scrolled-doc',
            manager: 'continuous',
            spread: 'none'
        });
        await rendition.display();
    }

    async function renderDjvuCover(element, url) {
        const viewer = new window.DjVu.Viewer();
        viewer.render(element);
        await viewer.loadDocumentByUrl(url, {
            viewMode: 'single',
            pageNumber: 1,
            pageScale: 0.35,
            language: 'ru',
            uiOptions: {
                hideFullPageSwitch: true,
                hideOpenAndCloseButtons: true,
                hidePrintButton: true,
                hideSaveButton: true,
                showContentsAutomatically: false
            }
        });
        const toolbar = element.querySelector('[data-djvujs-id="toolbar"]');
        if (toolbar) toolbar.style.display = 'none';
    }

    async function renderDocxCover(element, url) {
        const response = await fetch(url);
        if (!response.ok) throw new Error(`HTTP ${response.status}`);
        const stage = document.createElement('div');
        stage.className = 'docx-cover-stage';
        element.replaceChildren(stage);
        await docx.renderAsync(await response.blob(), stage, null, { breakPages: true });

        const wrapper = stage.querySelector('.docx-wrapper');
        const firstPage = wrapper?.querySelector('section.docx');
        if (!wrapper || !firstPage) throw new Error('DOCX first page was not rendered');

        wrapper.replaceChildren(firstPage);
        wrapper.style.cssText = 'position:absolute;inset:0;display:block;padding:0;background:transparent';
        firstPage.style.position = 'absolute';
        firstPage.style.margin = '0';
        firstPage.style.transformOrigin = 'top left';

        const scale = Math.max(
            element.clientWidth / firstPage.offsetWidth,
            element.clientHeight / firstPage.offsetHeight
        );
        firstPage.style.left = `${(element.clientWidth - firstPage.offsetWidth * scale) / 2}px`;
        firstPage.style.top = `${(element.clientHeight - firstPage.offsetHeight * scale) / 2}px`;
        firstPage.style.transform = `scale(${scale})`;
    }

    async function renderTextCover(element, url) {
        const response = await fetch(url);
        if (!response.ok) throw new Error(`HTTP ${response.status}`);
        const preview = document.createElement('pre');
        preview.className = 'doc-cover-text';
        preview.textContent = (await response.text()).slice(0, 2200);
        element.replaceChildren(preview);
    }

    async function renderCatalogCover(element) {
        if (element.dataset.coverLoaded === 'true') return;
        element.dataset.coverLoaded = 'true';
        const format = (element.dataset.coverFormat || '').toLowerCase();
        const url = element.dataset.coverUrl;
        const title = element.getAttribute('aria-label')?.replace(/^Первая страница:\s*/, '');
        try {
            if (format === 'pdf') await renderPdfCover(element, url);
            else if (format === 'epub') await renderEpubCover(element, url);
            else if (format === 'djvu') await renderDjvuCover(element, url);
            else if (format === 'docx') await renderDocxCover(element, url);
            else if (format === 'txt') await renderTextCover(element, url);
            else showCoverFallback(element, title, format);
        } catch (error) {
            console.warn(`Cover preview failed for document ${element.dataset.coverId}:`, error);
            showCoverFallback(element, title, format);
        }
    }

    function observeCatalogCovers() {
        const elements = [...document.querySelectorAll('.doc-cover[data-cover-url]:not([data-cover-observed])')];
        if (!elements.length) return;
        if (!('IntersectionObserver' in window)) {
            elements.forEach(renderCatalogCover);
            return;
        }
        coverObserver ??= new IntersectionObserver(entries => {
            for (const entry of entries) {
                if (!entry.isIntersecting) continue;
                coverObserver.unobserve(entry.target);
                coverQueue = coverQueue.then(() => renderCatalogCover(entry.target));
            }
        }, { rootMargin: '240px 0px' });
        for (const element of elements) {
            element.dataset.coverObserved = 'true';
            coverObserver.observe(element);
        }
    }

    function getSnapshot() {
        if (activeViewer?.kind === 'docx' || activeViewer?.kind === 'txt')
            updateScrollablePageState(activeViewer);
        return {
            currentPage: activeViewer?.currentPage ?? 1,
            totalPages: activeViewer?.totalPages ?? 1,
            zoom: activeViewer?.zoom ?? 1
        };
    }

    function getScrollHost(container) {
        return container.closest('.viewer-content') ?? container;
    }

    function updateScrollablePageState(viewer) {
        const host = viewer.scrollHost ?? getScrollHost(viewer.container);
        const pageHeight = Math.max(host.clientHeight, 1);
        viewer.scrollHost = host;
        viewer.totalPages = Math.max(1, Math.ceil(host.scrollHeight / pageHeight));
        viewer.currentPage = Math.min(viewer.totalPages, Math.floor(host.scrollTop / pageHeight) + 1);
    }

    function notifyState(viewer) {
        if (viewer.dotNetRef)
            viewer.dotNetRef.invokeMethodAsync('UpdateViewerState', viewer.currentPage, viewer.totalPages, viewer.zoom).catch(() => {});
    }

    function attachScrollTracking(viewer, dotNetRef) {
        viewer.dotNetRef = dotNetRef;
        viewer.scrollHost = getScrollHost(viewer.container);
        let scheduled = false;
        viewer.scrollHandler = () => {
            if (scheduled) return;
            scheduled = true;
            requestAnimationFrame(() => {
                scheduled = false;
                const previousPage = viewer.currentPage;
                updateScrollablePageState(viewer);
                if (viewer.currentPage !== previousPage) notifyState(viewer);
            });
        };
        viewer.scrollHost.addEventListener('scroll', viewer.scrollHandler, { passive: true });
        updateScrollablePageState(viewer);
    }

    function disposeActiveViewer() {
        if (!activeViewer) return;
        if (activeViewer.scrollHost && activeViewer.scrollHandler)
            activeViewer.scrollHost.removeEventListener('scroll', activeViewer.scrollHandler);
        if (activeViewer.kind === 'epub') {
            activeViewer.rendition.destroy();
            activeViewer.book.destroy();
        }
        if (activeViewer.kind === 'djvu') activeViewer.viewer.destroy();
        activeViewer = null;
    }

    function flattenToc(items, book, chapters = []) {
        for (const item of items ?? []) {
            if (item.href) {
                chapters.push({
                    href: item.href,
                    spineIndex: book.spine.get(item.href)?.index
                });
            }
            flattenToc(item.subitems, book, chapters);
        }
        return chapters;
    }

    function getEpubChapterNumber(viewer, location) {
        const href = location?.start?.href?.split('#')[0];
        const hrefName = href ? decodeURIComponent(href).split('/').pop() : '';
        let chapterIndex = viewer.chapters.findIndex(chapter =>
            decodeURIComponent(chapter.href.split('#')[0]).split('/').pop() === hrefName);
        if (chapterIndex < 0 && location?.start?.index !== undefined) {
            const spineIndex = Number(location.start.index);
            let nearestSpineIndex = -1;
            viewer.chapters.forEach((chapter, index) => {
                if (chapter.spineIndex <= spineIndex && chapter.spineIndex >= nearestSpineIndex) {
                    chapterIndex = index;
                    nearestSpineIndex = chapter.spineIndex;
                }
            });
        }
        return chapterIndex < 0 ? 1 : chapterIndex + 1;
    }

    function getEpubChapterAtViewport(viewer) {
        const host = viewer.container.closest('.viewer-content');
        const targetTop = host.getBoundingClientRect().top + 24;
        const frames = [...viewer.container.querySelectorAll('iframe')];
        const currentFrame = frames.find(frame => {
            const rect = frame.getBoundingClientRect();
            return rect.top <= targetTop && rect.bottom > targetTop;
        }) ?? frames.find(frame => frame.getBoundingClientRect().top > targetTop);
        if (!currentFrame) return viewer.currentPage;

        let sectionIndex = -1;
        try {
            const baseUrl = currentFrame.contentDocument.querySelector('base')?.href;
            const fileName = baseUrl ? new URL(baseUrl).pathname.split('/').pop() : '';
            sectionIndex = viewer.book.spine.spineItems.findIndex(section =>
                section.href.split('/').pop() === fileName);
        } catch {
            return viewer.currentPage;
        }

        let chapterIndex = -1;
        let nearestSpineIndex = -1;
        viewer.chapters.forEach((chapter, index) => {
            if (chapter.spineIndex <= sectionIndex && chapter.spineIndex >= nearestSpineIndex) {
                chapterIndex = index;
                nearestSpineIndex = chapter.spineIndex;
            }
        });
        return chapterIndex < 0 ? 1 : chapterIndex + 1;
    }

    function attachEpubScrollTracking(viewer, dotNetRef) {
        viewer.dotNetRef = dotNetRef;
        viewer.scrollHost = viewer.container.closest('.viewer-content');
        let scheduled = false;
        viewer.scrollHandler = () => {
            if (scheduled) return;
            scheduled = true;
            requestAnimationFrame(() => {
                scheduled = false;
                const page = getEpubChapterAtViewport(viewer);
                if (page !== viewer.currentPage) {
                    viewer.currentPage = page;
                    notifyState(viewer);
                }
            });
        };
        viewer.scrollHost.addEventListener('scroll', viewer.scrollHandler, { passive: true });
        viewer.scrollHandler();
    }

    async function renderPdfPage(pageNumber) {
        const viewer = activeViewer;
        const page = await viewer.document.getPage(pageNumber);
        const viewport = page.getViewport({ scale: viewer.zoom });
        const canvas = document.createElement('canvas');
        canvas.width = viewport.width;
        canvas.height = viewport.height;
        canvas.style.display = 'block';
        canvas.style.maxWidth = '100%';
        canvas.style.height = 'auto';
        canvas.style.margin = '0 auto';
        viewer.container.replaceChildren(canvas);
        const renderTask = page.render({ canvasContext: canvas.getContext('2d'), viewport });
        viewer.renderTask = renderTask;
        await renderTask.promise;
        viewer.currentPage = pageNumber;
    }

    async function initPdfViewer(containerId, url) {
        disposeActiveViewer();
        const container = document.getElementById(containerId);
        try {
            const pdfjsLib = await import('https://cdnjs.cloudflare.com/ajax/libs/pdf.js/4.4.168/pdf.min.mjs');
            pdfjsLib.GlobalWorkerOptions.workerSrc = 'https://cdnjs.cloudflare.com/ajax/libs/pdf.js/4.4.168/pdf.worker.min.mjs';
            const documentProxy = await pdfjsLib.getDocument(url).promise;
            activeViewer = {
                kind: 'pdf',
                container,
                document: documentProxy,
                currentPage: 1,
                totalPages: documentProxy.numPages,
                zoom: 1
            };
            await renderPdfPage(1);
        } catch (error) {
            container.innerHTML = `<p class="text-danger">Ошибка загрузки PDF: ${error.message}</p>`;
        }
        return getSnapshot();
    }

    async function initEpubViewer(containerId, url, dotNetRef) {
        disposeActiveViewer();
        const container = document.getElementById(containerId);
        try {
            const book = window.ePub(url, { openAs: 'epub' });
            await book.ready;
            const chapters = flattenToc(book.navigation.toc, book);
            if (!chapters.length) {
                chapters.push(...book.spine.spineItems.map(section => ({
                    href: section.href,
                    spineIndex: section.index
                })));
            }
            const rendition = book.renderTo(container, {
                width: '100%',
                height: '100%',
                flow: 'scrolled-doc',
                manager: 'continuous',
                spread: 'none'
            });
            await rendition.display();
            container.style.height = 'auto';
            container.style.minHeight = '100%';
            activeViewer = {
                kind: 'epub',
                container,
                book,
                rendition,
                chapters,
                currentPage: 1,
                totalPages: Math.max(chapters.length, 1),
                zoom: 1
            };
            const location = await rendition.currentLocation();
            activeViewer.currentPage = getEpubChapterNumber(activeViewer, location);
            rendition.on('relocated', nextLocation => {
                activeViewer.currentPage = getEpubChapterNumber(activeViewer, nextLocation);
                notifyState(activeViewer);
            });
            attachEpubScrollTracking(activeViewer, dotNetRef);
            rendition.themes.fontSize('100%');
        } catch (error) {
            container.innerHTML = `<p class="text-danger">Ошибка загрузки EPUB: ${error.message}</p>`;
        }
        return getSnapshot();
    }

    async function initDjvuViewer(containerId, url, dotNetRef) {
        disposeActiveViewer();
        const container = document.getElementById(containerId);
        try {
            const viewer = new window.DjVu.Viewer();
            viewer.render(container);
            await viewer.loadDocumentByUrl(url, {
                viewMode: 'single',
                language: 'ru',
                uiOptions: {
                    hideFullPageSwitch: true,
                    hideOpenAndCloseButtons: true,
                    hidePrintButton: true,
                    hideSaveButton: true,
                    showContentsAutomatically: false
                }
            });
            const state = viewer.store.getState();
            activeViewer = {
                kind: 'djvu',
                container,
                viewer,
                currentPage: viewer.getPageNumber(),
                totalPages: window.DjVu.Viewer.get.pagesQuantity(state),
                zoom: 1,
                dotNetRef
            };
            viewer.on(window.DjVu.Viewer.Events.PAGE_NUMBER_CHANGED, () => {
                activeViewer.currentPage = viewer.getPageNumber();
                notifyState(activeViewer);
            });
            const toolbar = container.querySelector('[data-djvujs-id="toolbar"]');
            if (toolbar) toolbar.style.display = 'none';
        } catch (error) {
            container.innerHTML = `<p class="text-danger">Ошибка загрузки DJVU: ${error.message}</p>`;
        }
        return getSnapshot();
    }

    async function initDocxViewer(containerId, url, dotNetRef) {
        disposeActiveViewer();
        const container = document.getElementById(containerId);
        try {
            const response = await fetch(url);
            if (!response.ok) throw new Error(`HTTP ${response.status}`);
            const blob = await response.blob();
            if (typeof docx === 'undefined' || !docx.renderAsync)
                throw new Error('Библиотека docx-preview не загружена');
            container.replaceChildren();
            await docx.renderAsync(blob, container);
            activeViewer = {
                kind: 'docx',
                container,
                currentPage: 1,
                zoom: 1
            };
            await new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve)));
            attachScrollTracking(activeViewer, dotNetRef);
        } catch (error) {
            container.innerHTML = `<p class="text-danger">Ошибка загрузки DOCX: ${error.message}</p>`;
        }
        return getSnapshot();
    }

    async function initTextViewer(containerId, url, dotNetRef) {
        disposeActiveViewer();
        const container = document.getElementById(containerId);
        try {
            const response = await fetch(url);
            if (!response.ok) throw new Error(`HTTP ${response.status}`);
            const content = await response.text();
            container.textContent = content;
            activeViewer = { kind: 'txt', container, currentPage: 1, zoom: 1 };
            await new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve)));
            attachScrollTracking(activeViewer, dotNetRef);
            return { ...getSnapshot(), content };
        } catch (error) {
            container.textContent = `Ошибка загрузки TXT: ${error.message}`;
            return { ...getSnapshot(), content: container.textContent };
        }
    }

    async function goToPage(pageNumber) {
        if (!activeViewer) return getSnapshot();
        const viewer = activeViewer;
        const page = Math.min(viewer.totalPages, Math.max(1, pageNumber));
        if (viewer.kind === 'pdf') {
            viewer.renderTask?.cancel();
            await renderPdfPage(page);
        } else if (viewer.kind === 'epub') {
            const chapter = viewer.chapters[page - 1];
            await viewer.rendition.display(chapter.href);
            const fileName = decodeURIComponent(chapter.href.split('#')[0]).split('/').pop();
            const frame = [...viewer.container.querySelectorAll('iframe')].find(item => {
                const baseUrl = item.contentDocument.querySelector('base')?.href;
                return baseUrl && new URL(baseUrl).pathname.split('/').pop() === fileName;
            });
            if (frame && viewer.scrollHost) {
                const offset = frame.getBoundingClientRect().top - viewer.scrollHost.getBoundingClientRect().top;
                viewer.scrollHost.scrollTop += offset;
                await new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve)));
            }
            const location = await viewer.rendition.currentLocation();
            viewer.currentPage = Math.max(page, getEpubChapterNumber(viewer, location));
        } else if (viewer.kind === 'djvu') {
            viewer.viewer.configure({ pageNumber: page });
            viewer.currentPage = page;
        } else if (viewer.kind === 'docx') {
            viewer.scrollHost.scrollTop = (page - 1) * viewer.scrollHost.clientHeight;
            viewer.currentPage = page;
        } else if (viewer.kind === 'txt') {
            viewer.scrollHost.scrollTop = (page - 1) * viewer.scrollHost.clientHeight;
            viewer.currentPage = page;
        }
        return getSnapshot();
    }

    async function setZoom(zoom) {
        if (!activeViewer) return getSnapshot();
        const viewer = activeViewer;
        viewer.zoom = Math.min(3, Math.max(0.25, zoom));
        if (viewer.kind === 'pdf') {
            viewer.renderTask?.cancel();
            await renderPdfPage(viewer.currentPage);
        } else if (viewer.kind === 'epub') {
            viewer.rendition.themes.fontSize(`${viewer.zoom * 100}%`);
        } else if (viewer.kind === 'djvu') {
            viewer.viewer.configure({ pageScale: viewer.zoom });
        } else if (viewer.kind === 'docx') {
            viewer.container.style.zoom = viewer.zoom;
        } else if (viewer.kind === 'txt') {
            viewer.container.style.fontSize = `${viewer.zoom * 100}%`;
        }
        if (viewer.kind === 'docx' || viewer.kind === 'txt') {
            const currentPage = viewer.currentPage;
            await new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve)));
            updateScrollablePageState(viewer);
            viewer.currentPage = Math.min(currentPage, viewer.totalPages);
            viewer.scrollHost.scrollTop = (viewer.currentPage - 1) * viewer.scrollHost.clientHeight;
            updateScrollablePageState(viewer);
        }
        return getSnapshot();
    }

    return {
        initPdfViewer,
        initEpubViewer,
        initDjvuViewer,
        initDocxViewer,
        initTextViewer,
        observeCatalogCovers,
        goToPage,
        setZoom,
        fetchText: async url => (await fetch(url)).text()
    };
})();
