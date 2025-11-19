import './App.css';
import ResumeChat from './ResumeChat';

function App() {
	return (
		<div>
			<ResumeChat />
			<footer className="footer">
				<div className="container text-muted">
					<span>
						&copy; 2025 r.russon consulting
						&nbsp; <a href="https://rrusson.com/RussonResume.pdf" target="_blank" className="text-muted">[hard copy]</a>
						&nbsp; <a href="https://github.com/rrusson/VirtualRyan" target="_blank" className="text-muted">[repo]</a>
					</span>
				</div>
			</footer>
		</div>
	);
}
export default App;