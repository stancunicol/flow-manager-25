// FlowManager AI Agent JavaScript Helpers
// These functions enable the AI Agent to interact with the application

window.flowManagerAgent = {
    
    // Navigation functions
    navigateToPage: function(url) {
        try {
            if (url.startsWith('/')) {
                window.location.href = url;
            } else {
                // Handle relative navigation
                window.location.href = '/' + url;
            }
            return true;
        } catch (error) {
            console.error('Navigation error:', error);
            return false;
        }
    },

    // Form interaction functions
    fillFormField: function(fieldName, value, fieldType = 'text') {
        try {
            // Try multiple selectors to find the field
            let field = null;
            
            // Try by name attribute
            field = document.querySelector(`[name="${fieldName}"]`);
            
            // Try by id attribute
            if (!field) {
                field = document.getElementById(fieldName);
            }
            
            // Try by placeholder text
            if (!field) {
                field = document.querySelector(`[placeholder*="${fieldName}" i]`);
            }
            
            // Try by label association
            if (!field) {
                const labels = document.querySelectorAll('label');
                for (let label of labels) {
                    if (label.textContent.toLowerCase().includes(fieldName.toLowerCase())) {
                        const forAttr = label.getAttribute('for');
                        if (forAttr) {
                            field = document.getElementById(forAttr);
                            break;
                        }
                    }
                }
            }
            
            // Try partial matching in various attributes
            if (!field) {
                field = document.querySelector(`input[name*="${fieldName}" i], textarea[name*="${fieldName}" i], select[name*="${fieldName}" i]`);
            }
            
            if (field) {
                // Handle different field types
                switch (field.type || fieldType.toLowerCase()) {
                    case 'checkbox':
                    case 'radio':
                        field.checked = value.toLowerCase() === 'true' || value === '1';
                        break;
                    case 'select-one':
                    case 'select-multiple':
                        // Try to find option by text or value
                        const options = field.querySelectorAll('option');
                        for (let option of options) {
                            if (option.value === value || option.textContent.toLowerCase().includes(value.toLowerCase())) {
                                option.selected = true;
                                break;
                            }
                        }
                        break;
                    default:
                        field.value = value;
                }
                
                // Trigger change event to update Blazor binding
                field.dispatchEvent(new Event('input', { bubbles: true }));
                field.dispatchEvent(new Event('change', { bubbles: true }));
                
                console.log(`Successfully filled field "${fieldName}" with value "${value}"`);
                return true;
            } else {
                console.warn(`Field "${fieldName}" not found`);
                return false;
            }
        } catch (error) {
            console.error('Error filling form field:', error);
            return false;
        }
    },

    // Button clicking functions - Enhanced with better detection
    clickButton: function(buttonText, buttonId = '', actionType = 'click') {
        try {
            console.log(`[AI Agent] Searching for button: "${buttonText}"`);
            let button = null;
            
            // Log all buttons on page for debugging
            const allButtons = document.querySelectorAll('button, input[type="button"], input[type="submit"], a.btn, .btn, a[role="button"]');
            console.log(`[AI Agent] Found ${allButtons.length} total buttons on page:`);
            allButtons.forEach((btn, index) => {
                console.log(`  ${index}: "${btn.textContent?.trim() || btn.value || 'No text'}" - classes: ${btn.className} - id: ${btn.id}`);
            });
            
            // Try by ID first
            if (buttonId) {
                button = document.getElementById(buttonId);
                console.log(`[AI Agent] ID search result: ${button ? 'Found' : 'Not found'}`);
            }
            
            // Try by exact text match (case insensitive)
            if (!button) {
                for (let btn of allButtons) {
                    const btnText = (btn.textContent?.trim() || btn.value || '').toLowerCase();
                    if (btnText === buttonText.toLowerCase()) {
                        button = btn;
                        console.log(`[AI Agent] Exact text match found: "${btnText}"`);
                        break;
                    }
                }
            }
            
            // Try by partial text match
            if (!button) {
                for (let btn of allButtons) {
                    const btnText = (btn.textContent?.trim() || btn.value || '').toLowerCase();
                    if (btnText.includes(buttonText.toLowerCase())) {
                        button = btn;
                        console.log(`[AI Agent] Partial text match found: "${btnText}"`);
                        break;
                    }
                }
            }
            
            // Try by aria-label or title attributes
            if (!button) {
                for (let btn of allButtons) {
                    const ariaLabel = (btn.getAttribute('aria-label') || '').toLowerCase();
                    const title = (btn.getAttribute('title') || '').toLowerCase();
                    if (ariaLabel.includes(buttonText.toLowerCase()) || title.includes(buttonText.toLowerCase())) {
                        button = btn;
                        console.log(`[AI Agent] Aria-label/title match found: "${ariaLabel || title}"`);
                        break;
                    }
                }
            }
            
            // Try to find by common link patterns (like "My Details" might be a link)
            if (!button) {
                const links = document.querySelectorAll('a');
                for (let link of links) {
                    const linkText = (link.textContent?.trim() || '').toLowerCase();
                    if (linkText.includes(buttonText.toLowerCase())) {
                        button = link;
                        console.log(`[AI Agent] Link match found: "${linkText}"`);
                        break;
                    }
                }
            }
            
            if (button) {
                if (!button.disabled && !button.classList.contains('disabled')) {
                    // Scroll into view first
                    button.scrollIntoView({ behavior: 'smooth', block: 'center' });
                    
                    // Multiple click attempts for better compatibility
                    button.focus();
                    button.click();
                    
                    // Also try triggering mouse events for stubborn elements
                    button.dispatchEvent(new MouseEvent('mousedown', { bubbles: true }));
                    button.dispatchEvent(new MouseEvent('mouseup', { bubbles: true }));
                    button.dispatchEvent(new MouseEvent('click', { bubbles: true }));
                    
                    console.log(`[AI Agent] Successfully clicked button: "${buttonText}"`);
                    return true;
                } else {
                    console.warn(`[AI Agent] Button "${buttonText}" found but disabled`);
                    return false;
                }
            } else {
                console.warn(`[AI Agent] Button "${buttonText}" not found anywhere on page`);
                return false;
            }
        } catch (error) {
            console.error('[AI Agent] Error clicking button:', error);
            return false;
        }
    },

    // Form submission
    submitCurrentForm: function() {
        try {
            // Look for submit buttons
            const submitButton = document.querySelector('button[type="submit"], input[type="submit"], button:contains("Submit"), button:contains("Send")');
            if (submitButton && !submitButton.disabled) {
                submitButton.click();
                return true;
            }
            
            // Try to submit the first form on the page
            const form = document.querySelector('form');
            if (form) {
                form.submit();
                return true;
            }
            
            console.warn('No submittable form found');
            return false;
        } catch (error) {
            console.error('Error submitting form:', error);
            return false;
        }
    },

    // Page information gathering
    getPageInfo: function(infoType = 'general') {
        try {
            let info = '';
            
            switch (infoType.toLowerCase()) {
                case 'forms':
                    const forms = document.querySelectorAll('form');
                    info = `Found ${forms.length} form(s) on the page:\n`;
                    forms.forEach((form, index) => {
                        const inputs = form.querySelectorAll('input, textarea, select');
                        info += `Form ${index + 1}: ${inputs.length} fields\n`;
                    });
                    break;
                    
                case 'buttons':
                    const buttons = document.querySelectorAll('button, input[type="button"], input[type="submit"], a.btn');
                    info = `Found ${buttons.length} button(s):\n`;
                    buttons.forEach((btn, index) => {
                        const text = btn.textContent.trim() || btn.value || btn.title || 'No text';
                        info += `${index + 1}. "${text}"\n`;
                    });
                    break;
                    
                case 'fields':
                    const fields = document.querySelectorAll('input, textarea, select');
                    info = `Found ${fields.length} form field(s):\n`;
                    fields.forEach((field, index) => {
                        const name = field.name || field.id || field.placeholder || 'No name';
                        const type = field.type || field.tagName.toLowerCase();
                        info += `${index + 1}. "${name}" (${type})\n`;
                    });
                    break;
                    
                default:
                    info = `Page: ${document.title}\n`;
                    info += `URL: ${window.location.pathname}\n`;
                    info += `Forms: ${document.querySelectorAll('form').length}\n`;
                    info += `Buttons: ${document.querySelectorAll('button, input[type="button"], input[type="submit"]').length}\n`;
                    info += `Input Fields: ${document.querySelectorAll('input, textarea, select').length}\n`;
            }
            
            return info;
        } catch (error) {
            console.error('Error getting page info:', error);
            return 'Error retrieving page information';
        }
    },

    // Content search
    searchPageContent: function(searchTerm) {
        try {
            const term = searchTerm.toLowerCase();
            let results = '';
            let count = 0;
            
            // Search in visible text
            const walker = document.createTreeWalker(
                document.body,
                NodeFilter.SHOW_TEXT,
                null,
                false
            );
            
            let node;
            while (node = walker.nextNode()) {
                if (node.textContent.toLowerCase().includes(term)) {
                    const parent = node.parentElement;
                    if (parent && parent.offsetParent !== null) { // Visible element
                        const context = node.textContent.trim();
                        if (context.length > 0) {
                            results += `${++count}. ${context.substring(0, 100)}...\n`;
                        }
                    }
                }
            }
            
            if (count === 0) {
                results = `No results found for "${searchTerm}"`;
            } else {
                results = `Found ${count} result(s) for "${searchTerm}":\n\n${results}`;
            }
            
            return results;
        } catch (error) {
            console.error('Error searching content:', error);
            return 'Error searching page content';
        }
    },

    // Utility functions
    waitForElement: function(selector, timeout = 5000) {
        return new Promise((resolve, reject) => {
            const element = document.querySelector(selector);
            if (element) {
                resolve(element);
                return;
            }
            
            const observer = new MutationObserver((mutations) => {
                const element = document.querySelector(selector);
                if (element) {
                    observer.disconnect();
                    resolve(element);
                }
            });
            
            observer.observe(document.body, {
                childList: true,
                subtree: true
            });
            
            setTimeout(() => {
                observer.disconnect();
                reject(new Error(`Element ${selector} not found within ${timeout}ms`));
            }, timeout);
        });
    },

    // Scroll to element
    scrollToElement: function(selector) {
        try {
            const element = document.querySelector(selector);
            if (element) {
                element.scrollIntoView({ behavior: 'smooth', block: 'center' });
                return true;
            }
            return false;
        } catch (error) {
            console.error('Error scrolling to element:', error);
            return false;
        }
    }
};

// Make functions available globally for Blazor JSInterop
window.navigateToPage = window.flowManagerAgent.navigateToPage;
window.fillFormField = window.flowManagerAgent.fillFormField;
window.clickButton = window.flowManagerAgent.clickButton;
window.submitCurrentForm = window.flowManagerAgent.submitCurrentForm;
window.getPageInfo = window.flowManagerAgent.getPageInfo;
window.searchPageContent = window.flowManagerAgent.searchPageContent;

console.log('FlowManager AI Agent JavaScript helpers loaded successfully');