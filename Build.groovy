pipeline {
	agent {
		label 'master'
	}

	stages {
		stage('Build') {
			steps {
				bat """
					\"%nuget_exe%\" restore

					CALL bin\\msbuild_where.cmd \"Photon.sln\" /m ^
						/p:Configuration=\"Release\" ^
						/p:Platform=\"Any CPU\" ^
						/target:Build
				"""
			}
		}
		stage('Test') {
			steps {
				bat """
					nunit3-console \"Photon.Tests\\bin\\Release\\Photon.Tests.dll\" ^
						--result=\"Photon.Tests\\bin\\Release\\TestResults.xml\" ^
						--where=\"cat == 'unit'\"
				"""
			}
			post {
				always {
					step([$class: 'NUnitPublisher',
						testResultsPattern: "Photon.Tests\\bin\\Release\\TestResults.xml",
						keepJUnitReports: true,
						skipJUnitArchiver: false,
						failIfNoResults: true,
						debug: false])
					
					archiveArtifacts artifacts: "Photon.Tests\\bin\\Release\\TestResults.xml"
				}
			}
		}
	}
}
